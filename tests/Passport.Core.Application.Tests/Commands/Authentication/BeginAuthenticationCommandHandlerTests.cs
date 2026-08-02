using NSubstitute;
using Xunit;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Authentication;

public class BeginAuthenticationCommandHandlerTests
{
    private readonly IUserQueryRepository _userQueryRepo = Substitute.For<IUserQueryRepository>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly BeginAuthenticationCommandHandler _handler;

    public BeginAuthenticationCommandHandlerTests()
    {
        _handler = new BeginAuthenticationCommandHandler(_userQueryRepo, _fido2, _challengeStore);
    }

    [Fact]
    public async Task HandleAsync_ValidEmailWithCredentials_ReturnsOptionsJson()
    {
        var command = new BeginAuthenticationCommand("test@example.com");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01"))));
        _userQueryRepo.GetCredentialsAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<CredentialInfo>>([new CredentialInfo([1], null, "2026-01-01")]));
        _fido2.CreateAssertionOptionsAsync(Arg.Any<byte[][]>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(string, string)>.Success(("{\"opts\":true}", "internal-state"))));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("{\"opts\":true}", result.Value.OptionsJson);
    }

    [Fact]
    public async Task HandleAsync_UserNotFound_ReturnsNotFound()
    {
        var command = new BeginAuthenticationCommand("test@example.com");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("user.not_found", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NoCredentials_ReturnsNotFound()
    {
        var command = new BeginAuthenticationCommand("test@example.com");
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01"))));
        _userQueryRepo.GetCredentialsAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IReadOnlyList<CredentialInfo>>([]));

        var result = await _handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("credentials.not_found", result.Error.Code);
    }
}