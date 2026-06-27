using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record EmailVerified(UserId UserId, string Email, DateTimeOffset OccurredAt) : DomainEvent;