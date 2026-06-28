using Xunit;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Tests.ValueObjects;

public class DeviceNameTests
{
    [Fact]
    public void Create_ValidName_ReturnsDeviceName()
    {
        var result = DeviceName.Create("iPhone 15");

        Assert.True(result.IsSuccess);
        Assert.Equal("iPhone 15", result.Value.Value);
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var result = DeviceName.Create("  YubiKey  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("YubiKey", result.Value.Value);
    }

    [Fact]
    public void Create_EmptyName_ReturnsValidationError()
    {
        var result = DeviceName.Create("");

        Assert.True(result.IsFailure);
        Assert.Equal("device_name.empty", result.Error.Code);
    }

    [Fact]
    public void Create_TooLong_ReturnsValidationError()
    {
        var longName = new string('a', 101);

        var result = DeviceName.Create(longName);

        Assert.True(result.IsFailure);
        Assert.Equal("device_name.too_long", result.Error.Code);
    }

    [Fact]
    public void Create_ControlCharacters_ReturnsValidationError()
    {
        var result = DeviceName.Create("test\0name");

        Assert.True(result.IsFailure);
        Assert.Equal("device_name.invalid_chars", result.Error.Code);
    }

    [Fact]
    public void Create_MaxLength_Succeeds()
    {
        var name = new string('a', 100);

        var result = DeviceName.Create(name);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = DeviceName.Create("YubiKey").Value;
        var b = DeviceName.Create("YubiKey").Value;

        Assert.Equal(a, b);
    }
}