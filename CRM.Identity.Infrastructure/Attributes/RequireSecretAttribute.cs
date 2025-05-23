namespace CRM.Identity.Infrastructure.Attributes;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireSecretAttribute : Attribute;