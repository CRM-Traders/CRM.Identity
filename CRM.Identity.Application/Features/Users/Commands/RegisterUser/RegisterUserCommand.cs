namespace CRM.Identity.Application.Features.Users.Commands.RegisterUser;

public sealed record RegisterUserCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? PhoneNumber) : IRequest<Unit>;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("First name is required and cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(50)
            .WithMessage("Last name is required and cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .Password();

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^[+]?[\d\s()-]+$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Phone number format is invalid.");
    }
}

public sealed class RegisterUserCommandHandler(
    IRepository<User> _userRepository,
    IPasswordService _passwordService,
    IUnitOfWork _unitOfWork) : IRequestHandler<RegisterUserCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var userSpecification = new UserByEmailSpecification(request.Email.Trim().ToLower());
        var existingUser = await _userRepository.FirstOrDefaultAsync(userSpecification, cancellationToken);

        if (existingUser != null)
        {
            return Result.Failure<Unit>("User with this email already exists", "Conflict");
        }

        var passwordHash = _passwordService.HashPasword(request.Password, out var salt);
        var saltBase64 = Convert.ToBase64String(salt);

        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email.Trim().ToLower(),
            request.PhoneNumber,
            passwordHash,
            saltBase64);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(Unit.Value);
    }
}