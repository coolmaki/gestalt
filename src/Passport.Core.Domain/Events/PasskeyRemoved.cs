using Gestalt.Lib.Domain;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Events;

public sealed record PasskeyRemoved(Email Email, byte[] CredentialId, DateTimeOffset OccurredAt) : DomainEvent;