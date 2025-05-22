namespace CRM.Identity.Application.Common.Specifications.Users;

public class UserByEmailSpecification(string email) : BaseSpecification<User>(user => user.Email == email);