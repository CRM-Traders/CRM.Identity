namespace CRM.Identity.Domain.Common.Options.Redis;

public class RedisOptions
{
    public string ConnectionString { get; set; } = null!;
    public int SessionTimeout { get; set; }
    public string Password { get; set; } = null!;
}
