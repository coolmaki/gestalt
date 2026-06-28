namespace Passport.Infrastructure;

public sealed class PassportInfrastructureConfiguration
{
    public const string SectionName = "Passport:Infrastructure";

    public string Provider { get; set; } = "Sqlite";

    public string ConnectionString { get; set; } = "Data Source=passport.db";
}