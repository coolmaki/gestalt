using Microsoft.EntityFrameworkCore;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;
using Passport.Infrastructure.Persistence;

namespace Passport.Infrastructure.Repositories;

internal sealed class RecoveryCodeRepository(PassportDbContext dbContext) : IRecoveryCodeRepository
{
    public async Task<Option<RecoveryCode>> FindActiveByEmailAsync(
        Email email, RecoveryCodePurpose purpose, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var codes = await dbContext.RecoveryCodes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var matched = codes.FirstOrDefault(rc =>
            string.Equals(rc.Email.Value, email.Value, StringComparison.OrdinalIgnoreCase) &&
            rc.Purpose == purpose &&
            rc.UsedAt == null &&
            rc.ExpiresAt > now);

        return matched is null ? Option<RecoveryCode>.None : matched;
    }

    public async Task AddAsync(RecoveryCode code, CancellationToken cancellationToken)
    {
        await dbContext.RecoveryCodes.AddAsync(code, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}