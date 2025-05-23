using CRM.Identity.Application.Common.Specifications.TradingAccounts;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.TradingAccounts;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Application.Features.TradingAccounts.Commands.CreateTradingAccount;

public sealed record CreateTradingAccountCommand(
    string AccountLogin,
    Currency Currency,
    decimal Balance,
    string? Leverage,
    string? Server,
    decimal Equity,
    bool IsDemo,
    Guid ClientId) : IRequest<Guid>;

public sealed class CreateTradingAccountCommandValidator : AbstractValidator<CreateTradingAccountCommand>
{
    public CreateTradingAccountCommandValidator()
    {
        RuleFor(x => x.AccountLogin)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Account login is required and cannot exceed 50 characters.");

        RuleFor(x => x.Currency)
            .IsInEnum()
            .WithMessage("Valid currency is required.");

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

        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required.");
    }
}

public sealed class CreateTradingAccountCommandHandler(
    IRepository<TradingAccount> tradingAccountRepository,
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateTradingAccountCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateTradingAccountCommand request,
        CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.ClientId, cancellationToken);
        if (client == null)
        {
            return Result.Failure<Guid>("Client not found", "NotFound");
        }

        var accountLoginSpecification = new TradingAccountByLoginSpecification(request.AccountLogin);
        var existingAccount =
            await tradingAccountRepository.FirstOrDefaultAsync(accountLoginSpecification, cancellationToken);

        if (existingAccount != null)
        {
            return Result.Failure<Guid>("Trading account with this login already exists", "Conflict");
        }

        var tradingAccount = new TradingAccount
        {
            AccountLogin = request.AccountLogin,
            Currency = request.Currency,
            Balance = request.Balance,
            Leverage = request.Leverage,
            Server = request.Server,
            Equity = request.Equity,
            IsDemo = request.IsDemo,
            ClientId = request.ClientId
        };

        await tradingAccountRepository.AddAsync(tradingAccount, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(tradingAccount.Id);
    }
}