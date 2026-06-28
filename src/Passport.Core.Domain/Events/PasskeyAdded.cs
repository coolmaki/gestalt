using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record PasskeyAdded(string Email, byte[] CredentialId, DateTimeOffset OccurredAt) : DomainEvent;