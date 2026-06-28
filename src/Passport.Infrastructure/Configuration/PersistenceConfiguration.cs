namespace Passport.Infrastructure.Configuration;

public sealed class PersistenceConfiguration
{
    public PersistenceProvider Provider { get; set; } = PersistenceProvider.Sqlite;

    public string ConnectionString { get; set; } = "Data Source=passport.db";
}