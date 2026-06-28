using Supercluster.Lib.Domain;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Events;

public sealed record EmailVerified(Email Email, DateTimeOffset OccurredAt) : DomainEvent;