using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record UserRegistered(string Email, DateTimeOffset OccurredAt) : DomainEvent;