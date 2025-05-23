using CRM.Identity.Domain.Entities.Clients;
using CRM.Identity.Domain.Entities.TradingAccounts.Enums;

namespace CRM.Identity.Domain.Entities.TradingAccounts;

public class TradingAccount : AuditableEntity
{
    public required string AccountLogin { get; set; }
    public Currency Currency { get; set; }
    public decimal Balance { get; set; }
    public string? Leverage { get; set; }
    public string? Server { get; set; }
    public decimal Equity { get; set; }
    public bool IsDemo { get; set; }

    public Guid ClientId { get; set; }
    [ForeignKey("ClientId")] public virtual Client? Client { get; set; }
}