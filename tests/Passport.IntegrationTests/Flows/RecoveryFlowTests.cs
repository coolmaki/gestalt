using System.Net.Http.Json;
using Xunit;
using System.Text.Json;

namespace Passport.IntegrationTests.Flows;

public class RecoveryFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public RecoveryFlowTests()
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

    private async Task RegisterAndVerifyAsync()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });

        var code = _host.GetLastCode()!;
        await _client.PostAsJsonAsync("/api/v1/auth/register/verify-email", new { Email = "test@example.com", Code = code });
    }

    [Fact]
    public async Task RecoveryFlow_FullCycle_CompletesSuccessfully()
    {
        await RegisterAndVerifyAsync();

        var beginResponse = await _client.PostAsJsonAsync("/api/v1/auth/recovery/begin", new { Email = "test@example.com" });
        Assert.True(beginResponse.IsSuccessStatusCode);

        var code = _host.GetLastCode()!;

        var verifyResponse = await _client.PostAsJsonAsync("/api/v1/auth/recovery/verify-code", new { Email = "test@example.com", Code = code });
        var verifyBody = await verifyResponse.Content.ReadAsStringAsync();
        Assert.True(verifyResponse.IsSuccessStatusCode, $"Verify code returned {verifyResponse.StatusCode}: {verifyBody}");
        var verifyResult = JsonSerializer.Deserialize<JsonElement>(verifyBody);
        var recoveryToken = verifyResult.GetProperty("recoveryToken").GetString()!;

        var beginRegResponse = await _client.PostAsJsonAsync("/api/v1/auth/recovery/begin-registration", new { RecoveryToken = recoveryToken });
        var beginRegBody = await beginRegResponse.Content.ReadAsStringAsync();
        Assert.True(beginRegResponse.IsSuccessStatusCode, $"Begin registration returned {beginRegResponse.StatusCode}: {beginRegBody}");

        var completeResponse = await _client.PostAsJsonAsync("/api/v1/auth/recovery/complete", new { RecoveryToken = recoveryToken, AttestationJson = "{}" });
        var completeBody = await completeResponse.Content.ReadAsStringAsync();
        Assert.True(completeResponse.IsSuccessStatusCode, $"Complete recovery returned {completeResponse.StatusCode}: {completeBody}");
    }

    [Fact]
    public async Task RecoveryFlow_WrongCode_ReturnsBadRequest()
    {
        await RegisterAndVerifyAsync();

        await _client.PostAsJsonAsync("/api/v1/auth/recovery/begin", new { Email = "test@example.com" });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/recovery/verify-code", new { Email = "test@example.com", Code = "000000" });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}