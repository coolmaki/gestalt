using System.Net.Http.Json;
using Xunit;
using System.Text.Json;

namespace Passport.IntegrationTests.Flows;

public class TokenFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public TokenFlowTests()
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

    [Fact]
    public async Task RefreshFlow_ValidToken_ReturnsNewPair()
    {
        // Register + verify email
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });
        var code = _host.GetLastCode()!;
        await _client.PostAsJsonAsync("/api/v1/auth/register/verify-email", new { Email = "test@example.com", Code = code });

        // Authenticate
        await _client.PostAsJsonAsync("/api/v1/auth/login/begin", new { Email = "test@example.com" });
        var loginResponse = await _client.PostAsJsonAsync("/api/v1/auth/login/complete", new { Email = "test@example.com", AssertionJson = "{}" });
        var loginBody = await loginResponse.Content.ReadAsStringAsync();
        Assert.True(loginResponse.IsSuccessStatusCode, $"Login returned {loginResponse.StatusCode}: {loginBody}");
        var loginResult = JsonSerializer.Deserialize<JsonElement>(loginBody);
        var refreshToken = loginResult.GetProperty("refreshToken").GetString()!;

        // Refresh
        var response = await _client.PostAsJsonAsync("/api/v1/auth/token/refresh", new { RefreshToken = refreshToken });
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"Refresh returned {response.StatusCode}: {body}");

        var result = JsonSerializer.Deserialize<JsonElement>(body);
        Assert.True(result.TryGetProperty("accessToken", out _));
        Assert.True(result.TryGetProperty("refreshToken", out _));
    }

    [Fact]
    public async Task JwksEndpoint_ReturnsValidKeys()
    {
        var response = await _client.GetAsync("/.well-known/jwks.json");
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"JWKS returned {response.StatusCode}: {body}");

        var jwks = JsonSerializer.Deserialize<JsonElement>(body);
        var keys = jwks.GetProperty("keys");
        Assert.Equal(1, keys.GetArrayLength());
        var key = keys[0];
        Assert.Equal("EC", key.GetProperty("kty").GetString());
        Assert.Equal("ES256", key.GetProperty("alg").GetString());
        Assert.Equal("P-256", key.GetProperty("crv").GetString());
    }
}