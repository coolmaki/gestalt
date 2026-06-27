using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record PasskeyAdded(UserId UserId, byte[] CredentialId, DateTimeOffset OccurredAt) : DomainEvent;