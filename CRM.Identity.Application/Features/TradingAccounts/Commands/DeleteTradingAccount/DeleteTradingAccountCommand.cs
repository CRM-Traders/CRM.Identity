using CRM.Identity.Domain.Entities.TradingAccounts;

namespace CRM.Identity.Application.Features.TradingAccounts.Commands.DeleteTradingAccount;

public sealed record DeleteTradingAccountCommand(Guid Id) : IRequest<Unit>;

public sealed class DeleteTradingAccountCommandValidator : AbstractValidator<DeleteTradingAccountCommand>
{
    public DeleteTradingAccountCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Trading Account ID is required.");
    }
}

public sealed class DeleteTradingAccountCommandHandler(
    IRepository<TradingAccount> tradingAccountRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<DeleteTradingAccountCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(DeleteTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var tradingAccount = await tradingAccountRepository.GetByIdAsync(request.Id, cancellationToken);
        if (tradingAccount == null)
        {
            return Result.Failure<Unit>("Trading account not found", "NotFound");
        }

        await tradingAccountRepository.DeleteAsync(tradingAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}