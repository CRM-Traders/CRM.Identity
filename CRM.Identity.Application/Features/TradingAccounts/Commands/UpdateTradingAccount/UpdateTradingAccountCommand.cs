using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Features.TradingAccounts.Commands.UpdateTradingAccount;

public sealed record UpdateTradingAccountCommand(
    Guid Id,
    decimal Balance,
    string? Leverage,
    string? Server,
    decimal Equity) : IRequest<Unit>;

public sealed class UpdateTradingAccountCommandValidator : AbstractValidator<UpdateTradingAccountCommand>
{
    public UpdateTradingAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Trading Account ID is required.");

        RuleFor(x => x.Balance)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Balance cannot be negative.");

        RuleFor(x => x.Leverage)
            .MaximumLength(20)
            .When(x => !string.IsNullOrEmpty(x.Leverage))
            .WithMessage("Leverage cannot exceed 20 characters.");

        RuleFor(x => x.Server)
            .MaximumLength(100)
            .When(x => !string.IsNullOrEmpty(x.Server))
            .WithMessage("Server cannot exceed 100 characters.");

        RuleFor(x => x.Equity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Equity cannot be negative.");
    }
}

public sealed class UpdateTradingAccountCommandHandler(
    IRepository<TradingAccount> tradingAccountRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<UpdateTradingAccountCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(UpdateTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var tradingAccount = await tradingAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tradingAccount == null)
        {
            return Result.Failure<Unit>("Trading account not found", "NotFound");
        }

        tradingAccount.Balance = request.Balance;
        tradingAccount.Leverage = request.Leverage;
        tradingAccount.Server = request.Server;
        tradingAccount.Equity = request.Equity;

        await tradingAccountRepository.UpdateAsync(tradingAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}