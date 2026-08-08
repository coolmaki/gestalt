using Microsoft.EntityFrameworkCore;
using Gestalt.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;
using Passport.Infrastructure.Persistence;

namespace Passport.Infrastructure.Repositories;

internal sealed class UserCommandRepository(PassportDbContext dbContext) : IUserCommandRepository
{
    public async Task<Option<User>> FindByEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .Include(u => u.Passkeys)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return user is null ? Option<User>.None : user;
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        await dbContext.Users.AddAsync(user, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}