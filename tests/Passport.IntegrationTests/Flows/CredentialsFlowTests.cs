using System.Net.Http.Json;
using Xunit;
using System.Text.Json;

namespace Passport.IntegrationTests.Flows;

public class CredentialsFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public CredentialsFlowTests()
    {
        _host = TestHostFactory.Create();
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

    private async Task<string> RegisterAndAuthenticateAsync()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });

        var code = _host.GetLastCode()!;
        await _client.PostAsJsonAsync("/api/v1/auth/register/verify-email", new { Email = "test@example.com", Code = code });

        await _client.PostAsJsonAsync("/api/v1/auth/login/begin", new { Email = "test@example.com" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login/complete", new { Email = "test@example.com", AssertionJson = "{}" });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginBody);
        return loginResult.GetProperty("accessToken").GetString()!;
    }

    [Fact]
    public async Task GetCredentials_ReturnsPasskeyList()
    {
        var accessToken = await RegisterAndAuthenticateAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/v1/auth/credentials?email=test@example.com");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);

        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Get credentials returned {response.StatusCode}: {body}");
    }

    [Fact]
    public async Task RemoveCredential_LastCredential_ReturnsConflict()
    {
        var accessToken = await RegisterAndAuthenticateAsync();

        var request = new HttpRequestMessage(HttpMethod.Delete, "/api/v1/auth/credentials")
        {
            Content = new StringContent(
                """{"email":"test@example.com","credentialId":"AQID"}""",
                System.Text.Encoding.UTF8,
                "application/json"),
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
        var response = await _client.SendAsync(request);

        Assert.Equal(System.Net.HttpStatusCode.Conflict, response.StatusCode);
    }
}