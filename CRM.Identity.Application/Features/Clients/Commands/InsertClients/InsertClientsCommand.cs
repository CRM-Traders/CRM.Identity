using System.Security.Cryptography;
using System.Text;
using CRM.Identity.Domain.Entities.Affiliate;
using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.Leads;

namespace CRM.Identity.Application.Features.Clients.Commands.InsertClients;

public sealed record InsertClientsCommand(List<InsertClientRequest> Clients) : IRequest<InsertClientsResult>;

public sealed record InsertClientRequest(
    string FirstName,
    string LastName,
    string Email,
    string Username,
    Guid AffiliateId,
    string? Telephone = null,
    string? Country = null,
    string? Language = null,
    DateTime? DateOfBirth = null,
    string? Source = null);

public sealed record InsertClientsResult(
    int SuccessCount,
    int FailureCount,
    List<InsertClientResult> ClientResults,
    List<string> Errors);

public sealed record InsertClientResult(
    string FirstName,
    string LastName,
    string Email,
    string GeneratedPassword,
    Guid ClientId,
    Guid UserId,
    Guid AffiliateId);

// ================================================================================================
// Validator
// ================================================================================================
public sealed class InsertClientsCommandValidator : AbstractValidator<InsertClientsCommand>
{
    public InsertClientsCommandValidator()
    {
        RuleFor(x => x.Clients)
            .NotNull()
            .WithMessage("Clients list is required")
            .NotEmpty()
            .WithMessage("At least one client is required")
            .Must(x => x.Count <= 1000)
            .WithMessage("Maximum 1000 clients allowed per request")
            .Must(HaveUniqueEmails)
            .WithMessage("Duplicate emails found in the request")
            .Must(HaveUniqueUsernames)
            .WithMessage("Duplicate usernames found in the request");

        RuleForEach(x => x.Clients).ChildRules(client =>
        {
            client.RuleFor(x => x.FirstName)
                .NotEmpty()
                .WithMessage("First name is required")
                .MaximumLength(100)
                .WithMessage("First name cannot exceed 100 characters");

            client.RuleFor(x => x.LastName)
                .NotEmpty()
                .WithMessage("Last name is required")
                .MaximumLength(100)
                .WithMessage("Last name cannot exceed 100 characters");

            client.RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required")
                .EmailAddress()
                .WithMessage("Invalid email format")
                .MaximumLength(255)
                .WithMessage("Email cannot exceed 255 characters");

            client.RuleFor(x => x.Username)
                .NotEmpty()
                .WithMessage("Username is required")
                .MinimumLength(3)
                .WithMessage("Username must be at least 3 characters")
                .MaximumLength(50)
                .WithMessage("Username cannot exceed 50 characters")
                .Matches("^[a-zA-Z0-9._-]+$")
                .WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

            client.RuleFor(x => x.AffiliateId)
                .NotEmpty()
                .WithMessage("Affiliate ID is required");

            client.RuleFor(x => x.Telephone)
                .MaximumLength(20)
                .WithMessage("Telephone cannot exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Telephone));

            client.RuleFor(x => x.Country)
                .MaximumLength(100)
                .WithMessage("Country cannot exceed 100 characters")
                .When(x => !string.IsNullOrEmpty(x.Country));

            client.RuleFor(x => x.Language)
                .MaximumLength(10)
                .WithMessage("Language cannot exceed 10 characters")
                .When(x => !string.IsNullOrEmpty(x.Language));

            client.RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.Today)
                .WithMessage("Date of birth must be in the past")
                .When(x => x.DateOfBirth.HasValue);
        });
    }

    private static bool HaveUniqueEmails(List<InsertClientRequest> clients)
    {
        var emails = clients.Select(c => c.Email.Trim().ToLower()).ToList();
        return emails.Count == emails.Distinct().Count();
    }

    private static bool HaveUniqueUsernames(List<InsertClientRequest> clients)
    {
        var usernames = clients.Select(c => c.Username.Trim().ToLower()).ToList();
        return usernames.Count == usernames.Distinct().Count();
    }
}

public sealed class InsertClientsCommandHandler(
    IRepository<Client> clientRepository,
    IRepository<Lead> leadRepository,
    IRepository<Affiliate> affiliateRepository,
    IRepository<User> userRepository,
    IPasswordService passwordService,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : IRequestHandler<InsertClientsCommand, InsertClientsResult>
{
    public async ValueTask<Result<InsertClientsResult>> Handle(InsertClientsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<string>();
        var clientResults = new List<InsertClientResult>();
        var successCount = 0;
        var failureCount = 0;

        try
        {
            // Pre-load all affiliates for validation
            var affiliateIds = request.Clients.Select(c => c.AffiliateId).Distinct().ToList();
            var affiliates = await affiliateRepository.GetByIdsAsync(affiliateIds, cancellationToken);
            var affiliateDict = affiliates.ToDictionary(a => a.Id);

            // Pre-check existing emails and usernames to avoid database round trips
            var emails = request.Clients.Select(c => c.Email.Trim().ToLower()).Distinct().ToList();
            var usernames = request.Clients.Select(c => c.Username.Trim().ToLower()).Distinct().ToList();

            var existingClients = await clientRepository.GetByEmailsAsync(emails, cancellationToken);
            var existingLeads = await leadRepository.GetByEmailsAsync(emails, cancellationToken);
            var existingUsers = await userRepository.GetByEmailsOrUsernamesAsync(emails, usernames, cancellationToken);

            var existingClientEmails = existingClients.Select(c => c.Email.ToLower()).ToHashSet();
            var existingLeadEmails = existingLeads.Select(l => l.Email.ToLower()).ToHashSet();
            var existingUserEmails = existingUsers.Select(u => u.Email.ToLower()).ToHashSet();
            var existingUsernames = existingUsers.Select(u => u.Username.ToLower()).ToHashSet();

            var clientIndex = 0;
            foreach (var clientRequest in request.Clients)
            {
                clientIndex++;
                try
                {
                    var email = clientRequest.Email.Trim().ToLower();
                    var username = clientRequest.Username.Trim().ToLower();

                    // Validate affiliate exists
                    if (!affiliateDict.ContainsKey(clientRequest.AffiliateId))
                    {
                        errors.Add($"Client {clientIndex}: Affiliate with ID {clientRequest.AffiliateId} not found");
                        failureCount++;
                        continue;
                    }

                    // Check duplicates
                    if (existingClientEmails.Contains(email))
                    {
                        errors.Add($"Client {clientIndex}: Client with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    if (existingLeadEmails.Contains(email))
                    {
                        errors.Add($"Client {clientIndex}: Lead with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    if (existingUserEmails.Contains(email))
                    {
                        errors.Add($"Client {clientIndex}: User with email {email} already exists");
                        failureCount++;
                        continue;
                    }

                    if (existingUsernames.Contains(username))
                    {
                        errors.Add($"Client {clientIndex}: User with username {username} already exists");
                        failureCount++;
                        continue;
                    }

                    // Create user and client
                    var generatedPassword = passwordService.GenerateStrongPassword();
                    var hashedPassword = passwordService.HashPasword(generatedPassword, out var salt);
                    var saltString = Convert.ToBase64String(salt);

                    var user = new User(
                        clientRequest.FirstName.Trim(),
                        clientRequest.LastName.Trim(),
                        email,
                        clientRequest.Username.Trim(), // სწორი username
                        clientRequest.Telephone?.Trim(),
                        hashedPassword,
                        saltString);

                    var client = new Client(
                        clientRequest.FirstName.Trim(),
                        clientRequest.LastName.Trim(),
                        email,
                        clientRequest.AffiliateId,
                        clientRequest.Telephone?.Trim(),
                        clientRequest.Country?.Trim(),
                        clientRequest.Language?.Trim(),
                        clientRequest.DateOfBirth,
                        userContext.IpAddress,
                        "API",
                        "JSON Insert",
                        clientRequest.Source?.Trim())
                    {
                        UserId = user.Id
                    };

                    await userRepository.AddAsync(user, cancellationToken);
                    await clientRepository.AddAsync(client, cancellationToken);

                    // Add to existing collections to prevent duplicates in the same batch
                    existingClientEmails.Add(email);
                    existingUserEmails.Add(email);
                    existingUsernames.Add(username);

                    clientResults.Add(new InsertClientResult(
                        clientRequest.FirstName.Trim(),
                        clientRequest.LastName.Trim(),
                        email,
                        generatedPassword,
                        client.Id,
                        user.Id,
                        clientRequest.AffiliateId));

                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add($"Client {clientIndex}: {ex.Message}");
                    failureCount++;
                }
            }

            if (successCount > 0)
            {
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }

            return Result.Success(new InsertClientsResult(successCount, failureCount, clientResults, errors));
        }
        catch (Exception ex)
        {
            return Result.Failure<InsertClientsResult>($"Error processing clients: {ex.Message}");
        }
    }
}