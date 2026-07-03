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
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var handler = new BeginRegistrationCommandHandler(userQueryRepo, Substitute.For<IFido2>(), Substitute.For<IChallengeStore>());

        var command = new BeginRegistrationCommand("test@example.com");
        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Option<UserReadModel>.Some(new UserReadModel("test@example.com", 1, "2026-01-01", "2026-01-01")));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("email.already_registered", result.Error.Code);
    }

    [Fact]
    public async Task HandleAsync_NewEmail_ReturnsOptionsJson()
    {
        var userQueryRepo = Substitute.For<IUserQueryRepository>();
        var fido2 = Substitute.For<IFido2>();
        var handler = new BeginRegistrationCommandHandler(userQueryRepo, fido2, Substitute.For<IChallengeStore>());

        var command = new BeginRegistrationCommand("test@example.com");
        userQueryRepo.FindByEmailAsync("test@example.com", Arg.Any<CancellationToken>())
            .Returns(Option<UserReadModel>.None);
        fido2.CreateRegistrationOptionsAsync(Arg.Any<Domain.ValueObjects.Email>(), Arg.Any<CancellationToken>())
            .Returns(Result<(string, byte[])>.Success(("{\"options\":true}", new byte[] { 1, 2, 3 })));

        var result = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task HandleAsync_InvalidEmail_ReturnsValidationError()
    {
        var handler = new BeginRegistrationCommandHandler(
            Substitute.For<IUserQueryRepository>(),
            Substitute.For<IFido2>(),
            Substitute.For<IChallengeStore>());

        var result = await handler.HandleAsync(new BeginRegistrationCommand("not-an-email"), CancellationToken.None);

        Assert.True(result.IsFailure);
    }
}