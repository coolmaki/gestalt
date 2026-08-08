using Gestalt.Lib.Primitives;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Repositories;

public interface IRecoveryCodeRepository
{
    /// <summary>
    /// Finds an active (unused, unexpired) recovery code for an email and purpose.
    /// </summary>
    Task<Option<RecoveryCode>> FindActiveByEmailAsync(Email email, RecoveryCodePurpose purpose, DateTimeOffset now, CancellationToken cancellationToken);

    /// <summary>
    /// Adds a new recovery code to the change tracker.
    /// </summary>
    Task AddAsync(RecoveryCode code, CancellationToken cancellationToken);

    /// <summary>
    /// Persists all tracked changes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken cancellationToken);
}