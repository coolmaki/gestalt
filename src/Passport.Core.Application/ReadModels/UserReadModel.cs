namespace Passport.Core.Application.ReadModels;

public sealed record UserReadModel(string Email, long EmailVerified, string CreatedAt, string UpdatedAt);