namespace Passport.Core.Application.ReadModels;

public sealed record UserReadModel(string Email, bool EmailVerified, DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt);