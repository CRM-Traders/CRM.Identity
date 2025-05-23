using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Clients.Enums;

namespace CRM.Identity.Application.Features.Clients.Commands.ChangeClientStatus;


public sealed record ChangeClientStatusCommand(
    Guid Id,
    ClientStatus Status) : IRequest<Unit>;

public sealed class ChangeClientStatusCommandValidator : AbstractValidator<ChangeClientStatusCommand>
{
    public ChangeClientStatusCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Client ID is required.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid client status.");
    }
}

public sealed class ChangeClientStatusCommandHandler(
    IRepository<Client> clientRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<ChangeClientStatusCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(ChangeClientStatusCommand request, CancellationToken cancellationToken)
    {
        var client = await clientRepository.GetByIdAsync(request.Id, cancellationToken);
        if (client == null)
        {
            return Result.Failure<Unit>("Client not found", "NotFound");
        }

        client.ChangeStatus(request.Status);

        await clientRepository.UpdateAsync(client, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}