using Gestalt.Lib.Domain;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Domain.Events;

public sealed record UserRegistered(Email Email, DateTimeOffset OccurredAt) : DomainEvent;