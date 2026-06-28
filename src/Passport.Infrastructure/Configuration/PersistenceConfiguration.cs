namespace Passport.Infrastructure.Configuration;

public sealed class PersistenceConfiguration
{
    public string Provider { get; set; } = "Sqlite";

    public string ConnectionString { get; set; } = "Data Source=passport.db";
}