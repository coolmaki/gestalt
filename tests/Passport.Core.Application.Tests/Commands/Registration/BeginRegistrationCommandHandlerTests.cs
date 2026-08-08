using NSubstitute;
using Xunit;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Registration;

public class BeginRegistrationCommandHandlerTests
{
    private readonly IUserQueryRepository _userQueryRepo = Substitute.For<IUserQueryRepository>();
    private readonly IFido2 _fido2 = Substitute.For<IFido2>();
    private readonly IChallengeStore _challengeStore = Substitute.For<IChallengeStore>();
    private readonly BeginRegistrationCommandHandler _handler;

    public BeginRegistrationCommandHandlerTests()
    {
        _handler = new BeginRegistrationCommandHandler(_userQueryRepo, _fido2, _challengeStore);
    }

    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsConflict()
    {
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Option<UserReadModel>.Some(new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01")));

        var result = await _handler.HandleAsync(new BeginRegistrationCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("email.already_registered", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_ReturnsOptionsJson()
    {
        _userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Option<UserReadModel>.None);
        _fido2.CreateRegistrationOptionsAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<CancellationToken>())
            .Returns(Result<(string, string)>.Success(("{\"options\":true}", "internal-state")));

        var result = await _handler.HandleAsync(new BeginRegistrationCommand("test@example.com"), CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ReturnsValidationError()
    {
        var result = await _handler.HandleAsync(new BeginRegistrationCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}