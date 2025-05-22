namespace CRM.Identity.Application.Features.Users.Commands.ChangePassword;

public sealed record ChangePasswordCommand(string OldPassword, string NewPassword, string ConfirmPassword) : IRequest<Unit>;

public sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand> 
{
    public ChangePasswordCommandValidator() 
    {
        RuleFor(x => x.NewPassword)
            .Password();

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword); 
    }
}

public sealed class ChangePasswordCommandHandler(
    IUnitOfWork _unitOfWork,
    IRepository<User> _userRepository,
    IPasswordService _passwordService,
    IUserContext _userContext) : IRequestHandler<ChangePasswordCommand, Unit>
{
    public async ValueTask<Result<Unit>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(_userContext.Id);

        if (user is null)
            return Result.Failure<Unit>("Can't find user");

        var oldPasswordHash = _passwordService.HashPasword(request.OldPassword, out _);

        if (user.PasswordHash != oldPasswordHash)
            return Result.Failure<Unit>("Invalid password");

        var newPasswordHash = _passwordService.HashPasword(request.NewPassword, out byte[] newPasswordSalt);

        user.ChangePassword(newPasswordHash, Convert.ToHexString(newPasswordSalt));

        await _unitOfWork.SaveChangesAsync();

        return Result.Success(Unit.Value);
    }
}
