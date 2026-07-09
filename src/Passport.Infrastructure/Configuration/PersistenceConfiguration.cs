namespace Passport.Infrastructure.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class PersistenceConfiguration
{
    public PersistenceProvider Provider { get; set; } = PersistenceProvider.Sqlite;

    public string ConnectionString { get; set; } = "Data Source=passport.db";
}