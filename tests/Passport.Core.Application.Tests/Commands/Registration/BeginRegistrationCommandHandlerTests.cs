using NSubstitute;
using Xunit;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands;
using Passport.Core.Application.ReadModels;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;

namespace Passport.Core.Application.Tests.Commands.Registration;

public class BeginRegistrationCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var fido2 = Substitute.For<IFido2>();
        var challengeStore = Substitute.For<IChallengeStore>();
        var handler = new BeginRegistrationCommandHandler(userQueryRepo, fido2, challengeStore);

        var command = new BeginRegistrationCommand("test@example.com");

        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.Some(
                new UserReadModel("test@example.com", true, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow))));

        // Ensure the duplicate check triggers — if this fails, it's a NSubstitute matching issue
        var directResult = await userQueryRepo.FindByEmailAsync("test@example.com", CancellationToken.None);
        Assert.True(directResult.IsSome);

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("email.already_registered", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_ReturnsOptionsJson()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var fido2 = Substitute.For<IFido2>();
        var challengeStore = Substitute.For<IChallengeStore>();
        var handler = new BeginRegistrationCommandHandler(userQueryRepo, fido2, challengeStore);

        var command = new BeginRegistrationCommand("test@example.com");

        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Option<UserReadModel>.None));
        fido2.CreateRegistrationOptionsAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result<(string, byte[])>.Success(("{\"options\":true}", new byte[] { 1, 2, 3 }))));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("{\"options\":true}", result.Value.OptionsJson);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ReturnsValidationError()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var fido2 = Substitute.For<IFido2>();
        var challengeStore = Substitute.For<IChallengeStore>();
        var handler = new BeginRegistrationCommandHandler(userQueryRepo, fido2, challengeStore);

        var command = new BeginRegistrationCommand("not-an-email");

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}