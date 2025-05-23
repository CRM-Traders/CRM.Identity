namespace CRM.Identity.Application.Common.Specifications.Users;

public class UserByUsernameSpecification(string username) : BaseSpecification<User>(user => user.Username == username);