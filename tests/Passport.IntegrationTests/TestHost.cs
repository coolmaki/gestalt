using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;
using Passport.Infrastructure.Configuration;
using Passport.Infrastructure.Persistence;

namespace Passport.IntegrationTests;

internal sealed class TestHost : WebApplicationFactory<Program>
{
    private readonly PersistenceConfiguration _persistenceConfig;

    public TestHost(PersistenceConfiguration persistenceConfig)
    {
        _persistenceConfig = persistenceConfig;
    }

    public async Task EnsureDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PassportDbContext>();
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        // Disable DI validation — test environment swaps services at runtime
        builder.UseSetting("ValidateOnBuild", "false");

        builder.ConfigureServices(services =>
        {
            // Remove the production registrations
            var codeDeliveryDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(ICodeDeliveryService));
            if (codeDeliveryDescriptor != null)
            {
                services.Remove(codeDeliveryDescriptor);
            }

            var fidoDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IFido2));
            if (fidoDescriptor != null)
            {
                services.Remove(fidoDescriptor);
            }

            var configDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(PersistenceConfiguration));
            if (configDescriptor != null)
            {
                services.Remove(configDescriptor);
            }

            // Replace with test implementations
            services.AddScoped<IFido2, TestFido2Service>();
            services.AddScoped<ICodeDeliveryService, CapturingCodeDeliveryService>();
            services.AddSingleton(_persistenceConfig);
        });
    }
}

internal sealed class CapturingCodeDeliveryService : ICodeDeliveryService
{
    public Email? LastEmail { get; private set; }
    public string? LastCode { get; private set; }

    public Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        LastEmail = to;
        LastCode = code;
        return Task.CompletedTask;
    }

    public Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken)
    {
        LastEmail = to;
        LastCode = code;
        return Task.CompletedTask;
    }
}