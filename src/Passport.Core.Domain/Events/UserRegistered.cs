using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record UserRegistered(UserId UserId, string Email, DateTimeOffset OccurredAt) : DomainEvent;