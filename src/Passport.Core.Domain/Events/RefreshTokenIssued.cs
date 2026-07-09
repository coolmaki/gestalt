using Passport.Core.Domain.ValueObjects;
using Supercluster.Lib.Domain;

namespace Passport.Core.Domain.Events;

public sealed record RefreshTokenIssued(Email Email, DateTimeOffset IssuedAt) : DomainEvent;