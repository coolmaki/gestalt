namespace Passport.Infrastructure.Configuration;

public sealed class PassportInfrastructureConfiguration
{
    public const string SectionName = "Passport:Infrastructure";

    public PersistenceConfiguration Persistence { get; set; } = new();
}