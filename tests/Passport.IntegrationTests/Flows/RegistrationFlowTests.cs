using System.Net.Http.Json;
using Xunit;

namespace Passport.IntegrationTests.Flows;

public class RegistrationFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public RegistrationFlowTests()
    {
        _host = TestHostFactory.Create();
    }

    public async Task InitializeAsync()
    {
        await _host.EnsureDatabaseAsync();
        _client = _host.CreateDefaultClient();
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
        var beginBody = await beginResponse.Content.ReadAsStringAsync();
        Assert.True(beginResponse.IsSuccessStatusCode, $"Begin returned {beginResponse.StatusCode}: {beginBody}");

        var completeResponse = await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });
        var completeBody = await completeResponse.Content.ReadAsStringAsync();
        Assert.True(completeResponse.IsSuccessStatusCode, $"Complete returned {completeResponse.StatusCode}: {completeBody}");
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