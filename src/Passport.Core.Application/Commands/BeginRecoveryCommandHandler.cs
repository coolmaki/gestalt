using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Repositories;
using Passport.Core.Application.Services;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands;

/// <summary>
/// Begins the account recovery flow. Sends a 6-digit recovery code to the user's
/// verified email. Silently succeeds if the email is not registered or not verified —
/// no user enumeration. See <see cref="VerifyRecoveryCodeCommandHandler"/>,
/// <see cref="BeginRecoveryRegistrationCommandHandler"/>,
/// <see cref="CompleteRecoveryCommandHandler"/>.
/// </summary>
internal sealed class BeginRecoveryCommandHandler(
    IUserQueryRepository userQueryRepo,
    IRecoveryCodeRepository recoveryCodeRepo,
    IEmailSender emailSender,
    IGuidProvider guids,
    IDateTimeProvider clock
) : ICommandHandler<BeginRecoveryCommand, Unit>
{
    private static readonly TimeSpan RecoveryCodeTtl = TimeSpan.FromMinutes(10);

    public async Task<Result<Unit>> HandleAsync(BeginRecoveryCommand command, CancellationToken ct)
    {
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            return Unit.Value;
        }

        Email email = emailResult.Value;

        var userOption = await userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (userOption.IsNone || !userOption.Value.EmailVerified)
        {
            return Unit.Value;
        }

        DateTimeOffset now = clock.UtcNow();
        string code = Helpers.GenerateCode();
        string codeHash = Helpers.HashCode(code);
        var recoveryCodeId = new RecoveryCodeId(guids.NewGuid());

        var recoveryCodeResult = Domain.Entities.RecoveryCode.Issue(recoveryCodeId, codeHash, RecoveryCodePurpose.AccountRecovery, now, RecoveryCodeTtl);
        if (recoveryCodeResult.IsFailure)
        {
            return recoveryCodeResult.Error;
        }

        await recoveryCodeRepo.AddAsync(recoveryCodeResult.Value, ct);
        await recoveryCodeRepo.SaveChangesAsync(ct);

        await emailSender.SendRecoveryCodeAsync(email, code, ct);

        return Unit.Value;
    }
}