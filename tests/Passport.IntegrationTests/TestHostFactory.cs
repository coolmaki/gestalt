using DotNet.Testcontainers.Containers;
using Passport.Infrastructure.Configuration;
using Testcontainers.PostgreSql;

namespace Passport.IntegrationTests;

internal static class TestHostFactory
{
    private static PostgreSqlContainer? _postgresContainer;

    public static TestHost Create()
    {
        var provider = Environment.GetEnvironmentVariable("PASSPORT_TEST_PROVIDER") ?? "Sqlite";
        return provider switch
        {
            "Sqlite" => CreateSqlite(),
            "Postgres" => CreatePostgres(),
            _ => throw new InvalidOperationException($"Unknown provider: {provider}"),
        };
    }

    public static async Task CleanupAsync()
    {
        if (_postgresContainer is not null)
        {
            await _postgresContainer.DisposeAsync();
        }
    }

    private static TestHost CreateSqlite()
    {
        return new TestHost(new PersistenceConfiguration
        {
            Provider = PersistenceProvider.Sqlite,
            ConnectionString = $"Data Source=test_{Guid.NewGuid()}.db",
        });
    }

    private static TestHost CreatePostgres()
    {
        _postgresContainer ??= new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .Build();

        if (_postgresContainer.State != TestcontainersStates.Running)
        {
            _postgresContainer.StartAsync().GetAwaiter().GetResult();
        }

        return new TestHost(new PersistenceConfiguration
        {
            Provider = PersistenceProvider.Postgres,
            ConnectionString = _postgresContainer.GetConnectionString(),
        });
    }
}