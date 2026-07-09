namespace Passport.Infrastructure.Configuration;

[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
public sealed class InfrastructureConfiguration
{
    public const string SectionName = "Passport:Infrastructure";

    public PersistenceConfiguration Persistence { get; set; } = new();
}