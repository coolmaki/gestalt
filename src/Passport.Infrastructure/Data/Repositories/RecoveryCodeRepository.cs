using Microsoft.EntityFrameworkCore;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Infrastructure.Data.Repositories;

internal sealed class RecoveryCodeRepository(PassportDbContext dbContext) : IRecoveryCodeRepository
{
    public async Task<Option<RecoveryCode>> FindActiveByEmailAsync(
        Email email, RecoveryCodePurpose purpose, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var code = await dbContext.RecoveryCodes
            .FirstOrDefaultAsync(rc =>
                rc.Email == email &&
                rc.Purpose == purpose &&
                rc.UsedAt == null &&
                rc.ExpiresAt > now,
                cancellationToken);

        return code is null ? Option<RecoveryCode>.None : code;
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