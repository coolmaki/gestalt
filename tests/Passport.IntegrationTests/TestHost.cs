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
            var emailSenderDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailSenderDescriptor != null)
            {
                services.Remove(emailSenderDescriptor);
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
            services.AddScoped<IEmailSender, NoOpEmailSender>();
            services.AddSingleton(_persistenceConfig);
        });
    }
}

internal sealed class NoOpEmailSender : IEmailSender
{
    public Task SendVerificationCodeAsync(Email to, string code, CancellationToken cancellationToken) => Task.CompletedTask;
    public Task SendRecoveryCodeAsync(Email to, string code, CancellationToken cancellationToken) => Task.CompletedTask;
}