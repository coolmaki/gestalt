namespace Passport.Core.Application.ReadModels;

public sealed record CredentialInfo(byte[] CredentialId, string? DeviceName, string CreatedAt);