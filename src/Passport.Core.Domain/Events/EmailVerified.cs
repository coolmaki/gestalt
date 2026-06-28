using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record EmailVerified(string Email, DateTimeOffset OccurredAt) : DomainEvent;