using Supercluster.Lib.Primitives;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Repositories;

public interface IUserCommandRepository
{
    /// <summary>
    /// Finds a user by email. Returns <see cref="Option{T}.None"/> if not found.
    /// </summary>
    Task<Option<User>> FindByEmailAsync(Email email, CancellationToken ct);

    /// <summary>
    /// Adds a new user to the change tracker.
    /// </summary>
    Task AddAsync(User user, CancellationToken ct);

    /// <summary>
    /// Persists all tracked changes.
    /// </summary>
    Task SaveChangesAsync(CancellationToken ct);
}