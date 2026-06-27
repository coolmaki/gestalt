using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record PasskeyRemoved(UserId UserId, byte[] CredentialId, DateTimeOffset OccurredAt) : DomainEvent;