using System.Net.Http.Json;
using Xunit;
using Passport.Infrastructure.Configuration;

namespace Passport.IntegrationTests.Flows;

public class RegistrationFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public RegistrationFlowTests()
    {
        var config = new PersistenceConfiguration
        {
            Provider = PersistenceProvider.Sqlite,
            ConnectionString = $"Data Source=test_passport_{Guid.NewGuid():N}.db",
        };

        _host = new TestHost(config);
    }

    public async Task InitializeAsync()
    {
        _client = _host.CreateDefaultClient();
        await _host.EnsureDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        _host.Dispose();
    }

    [Fact]
    public async Task RegisterFlow_ValidInput_ReturnsSuccess()
    {
        var beginResponse = await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        Assert.True(beginResponse.IsSuccessStatusCode);

        var completeResponse = await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });
        Assert.True(completeResponse.IsSuccessStatusCode, $"Complete returned {completeResponse.StatusCode}");
    }

    [Fact]
    public async Task RegisterFlow_DuplicateEmail_ReturnsConflict()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "dup@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "dup@example.com", AttestationJson = "{}" });

        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "dup@example.com" });
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "dup@example.com", AttestationJson = "{}" });

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task BeginRegistration_InvalidEmail_ReturnsBadRequest()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "not-an-email" });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}