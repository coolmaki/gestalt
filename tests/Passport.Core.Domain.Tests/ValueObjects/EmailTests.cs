using Xunit;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Tests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_ValidEmail_ReturnsEmail()
    {
        var result = Email.Create("test@example.com");

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value.Value);
    }

    [Fact]
    public void Create_ValidEmail_TrimsAndLowercases()
    {
        var result = Email.Create("  Test@Example.COM  ");

        Assert.True(result.IsSuccess);
        Assert.Equal("test@example.com", result.Value.Value);
    }

    [Fact]
    public void Create_EmptyEmail_ReturnsValidationError()
    {
        var result = Email.Create("");

        Assert.True(result.IsFailure);
        Assert.Equal("email.empty", result.Error.Code);
    }

    [Fact]
    public void Create_WhitespaceEmail_ReturnsValidationError()
    {
        var result = Email.Create("   ");

        Assert.True(result.IsFailure);
        Assert.Equal("email.empty", result.Error.Code);
    }

    [Fact]
    public void Create_NoAtSign_ReturnsValidationError()
    {
        var result = Email.Create("testexample.com");

        Assert.True(result.IsFailure);
        Assert.Equal("email.invalid_format", result.Error.Code);
    }

    [Fact]
    public void Create_NoDomain_ReturnsValidationError()
    {
        var result = Email.Create("test@");

        Assert.True(result.IsFailure);
        Assert.Equal("email.invalid_format", result.Error.Code);
    }

    [Fact]
    public void Create_NoTld_ReturnsValidationError()
    {
        var result = Email.Create("test@example");

        Assert.True(result.IsFailure);
        Assert.Equal("email.invalid_format", result.Error.Code);
    }

    [Fact]
    public void Create_TooLong_ReturnsValidationError()
    {
        var longLocal = new string('a', 250);
        var result = Email.Create($"{longLocal}@example.com");

        Assert.True(result.IsFailure);
        Assert.Equal("email.too_long", result.Error.Code);
    }

    [Fact]
    public void Equality_SameValue_AreEqual()
    {
        var a = Email.Create("test@example.com").Value;
        var b = Email.Create("test@example.com").Value;

        Assert.Equal(a, b);
        Assert.True(a == b);
    }

    [Fact]
    public void Equality_DifferentCase_AreEqual()
    {
        var a = Email.Create("Test@Example.com").Value;
        var b = Email.Create("test@example.com").Value;

        Assert.Equal(a, b);
    }
}