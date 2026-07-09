using System.Net.Http.Json;
using Xunit;
using System.Text.Json;

namespace Passport.IntegrationTests.Flows;

public class EmailVerificationFlowTests : IAsyncLifetime
{
    private readonly TestHost _host;
    private HttpClient _client = null!;

    public EmailVerificationFlowTests()
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
    public async Task VerifyEmail_ValidCode_VerifiesEmail()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });

        var code = _host.GetLastCode();
        Assert.NotNull(code);

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/verify-email", new { Email = "test@example.com", Code = code });
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task VerifyEmail_WrongCode_ReturnsBadRequest()
    {
        await _client.PostAsJsonAsync("/api/v1/auth/register/begin", new { Email = "test@example.com" });
        await _client.PostAsJsonAsync("/api/v1/auth/register/complete", new { Email = "test@example.com", AttestationJson = "{}" });

        var response = await _client.PostAsJsonAsync("/api/v1/auth/register/verify-email", new { Email = "test@example.com", Code = "000000" });

        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
    }
}