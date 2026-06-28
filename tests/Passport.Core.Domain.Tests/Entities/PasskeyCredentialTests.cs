using Xunit;
using Passport.Core.Domain.Entities;
using PD = Passport.Core.Domain;

namespace Passport.Core.Domain.Tests.Entities;

public class PasskeyCredentialTests
{
    private readonly DateTimeOffset _now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidInput_CreatesCredential()
    {
        var result = PD.Entities.PasskeyCredential.Create([1, 2, 3], [4, 5, 6], 0, _now);

        Assert.True(result.IsSuccess);
        Assert.Equal(new byte[] { 1, 2, 3 }, result.Value.CredentialId);
        Assert.Equal(new byte[] { 4, 5, 6 }, result.Value.PublicKey);
        Assert.Equal(0u, result.Value.SignCount);
    }

    [Fact]
    public void Create_EmptyCredentialId_ReturnsValidationError()
    {
        var result = PD.Entities.PasskeyCredential.Create([], [1], 0, _now);

        Assert.True(result.IsFailure);
        Assert.Equal("credential_id.empty", result.Error.Code);
    }

    [Fact]
    public void Create_EmptyPublicKey_ReturnsValidationError()
    {
        var result = PD.Entities.PasskeyCredential.Create([1], [], 0, _now);

        Assert.True(result.IsFailure);
        Assert.Equal("public_key.empty", result.Error.Code);
    }

    [Fact]
    public void UpdateSignCount_UpdatesValue()
    {
        var credential = PD.Entities.PasskeyCredential.Create([1], [1], 0, _now).Value;

        credential.UpdateSignCount(42);

        Assert.Equal(42u, credential.SignCount);
    }

    [Fact]
    public void SetDeviceName_SetsName()
    {
        var credential = PD.Entities.PasskeyCredential.Create([1], [1], 0, _now).Value;

        credential.SetDeviceName(PD.ValueObjects.DeviceName.Create("YubiKey").Value);

        Assert.Equal("YubiKey", credential.DeviceName!.Value);
    }

    [Fact]
    public void Equality_SameCredentialId_AreEqual()
    {
        var a = PD.Entities.PasskeyCredential.Create([1, 2], [1], 0, _now).Value;
        var b = PD.Entities.PasskeyCredential.Create([1, 2], [9], 0, _now).Value;

        Assert.Equal(a, b);
    }

    [Fact]
    public void Equality_DifferentCredentialId_AreNotEqual()
    {
        var a = PD.Entities.PasskeyCredential.Create([1, 2], [1], 0, _now).Value;
        var b = PD.Entities.PasskeyCredential.Create([3, 4], [1], 0, _now).Value;

        Assert.NotEqual(a, b);
    }
}