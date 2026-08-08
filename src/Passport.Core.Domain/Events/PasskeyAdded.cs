using Gestalt.Lib.Domain;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Events;

public sealed record PasskeyAdded(Email Email, byte[] CredentialId, DateTimeOffset OccurredAt) : DomainEvent;