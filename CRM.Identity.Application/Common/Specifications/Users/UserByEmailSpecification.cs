namespace CRM.Identity.Application.Common.Specifications.Users;

public class UserByEmailSpecification : BaseSpecification<User>
{
    public UserByEmailSpecification(string email)
        : base(user => user.Email == email)
    {
    }
}