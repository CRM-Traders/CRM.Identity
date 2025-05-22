using CRM.Identity.Application.Common.Specifications.Affiliates;
using CRM.Identity.Domain.Entities.Affiliate;

namespace CRM.Identity.Application.Features.Affiliates.Commands.CreateAffiliate;

public sealed record CreateAffiliateCommand(
    string Name,
    string Email,
    string? Phone,
    string? Website) : IRequest<Guid>;

public sealed class CreateAffiliateCommandValidator : AbstractValidator<CreateAffiliateCommand>
{
    public CreateAffiliateCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("Name is required and cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(200)
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^[+]?[\d\s()-]+$")
            .When(x => !string.IsNullOrEmpty(x.Phone))
            .WithMessage("Phone number format is invalid.");

        RuleFor(x => x.Website)
            .MaximumLength(200)
            .Matches(@"^https?://.+")
            .When(x => !string.IsNullOrEmpty(x.Website))
            .WithMessage("Website must be a valid URL.");
    }
}

public sealed class CreateAffiliateCommandHandler(
    IRepository<Affiliate> affiliateRepository,
    IUnitOfWork unitOfWork) : IRequestHandler<CreateAffiliateCommand, Guid>
{
    public async ValueTask<Result<Guid>> Handle(CreateAffiliateCommand request, CancellationToken cancellationToken)
    {
        var emailSpecification = new AffiliateByEmailSpecification(request.Email.Trim().ToLower());
        var existingAffiliate = await affiliateRepository.FirstOrDefaultAsync(emailSpecification, cancellationToken);

        if (existingAffiliate != null)
        {
            return Result.Failure<Guid>("Affiliate with this email already exists", "Conflict");
        }

        var affiliate = new Affiliate(
            request.Name.Trim(),
            request.Email.Trim().ToLower(),
            request.Phone?.Trim(),
            request.Website?.Trim());

        await affiliateRepository.AddAsync(affiliate, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(affiliate.Id);
    }
}