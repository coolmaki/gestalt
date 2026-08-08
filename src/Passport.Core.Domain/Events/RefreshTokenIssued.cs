using Passport.Core.Domain.ValueObjects;
using Gestalt.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record RefreshTokenIssued(Email Email, DateTimeOffset IssuedAt) : DomainEvent;