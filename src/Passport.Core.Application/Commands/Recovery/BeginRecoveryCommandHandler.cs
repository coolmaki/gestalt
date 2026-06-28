using System.Security.Cryptography;
using Supercluster.Lib.Application.Commands;
using Supercluster.Lib.Application.Providers;
using Supercluster.Lib.Primitives;
using Passport.Core.Application.Commands.Registration;
using Passport.Core.Application.Ports;
using Passport.Core.Domain.Entities;
using Passport.Core.Domain.ValueObjects;

namespace Passport.Core.Application.Commands.Recovery;

internal sealed class BeginRecoveryCommandHandler : ICommandHandler<BeginRecoveryCommand, Unit>
{
    private readonly IUserQueryRepository _userQueryRepo;
    private readonly IRecoveryCodeRepository _recoveryCodeRepo;
    private readonly IEmailSender _emailSender;
    private readonly IGuidProvider _guids;
    private readonly IDateTimeProvider _clock;

    private static readonly TimeSpan RecoveryCodeTtl = TimeSpan.FromMinutes(10);

    public BeginRecoveryCommandHandler(
        IUserQueryRepository userQueryRepo,
        IRecoveryCodeRepository recoveryCodeRepo,
        IEmailSender emailSender,
        IGuidProvider guids,
        IDateTimeProvider clock)
    {
        _userQueryRepo = userQueryRepo;
        _recoveryCodeRepo = recoveryCodeRepo;
        _emailSender = emailSender;
        _guids = guids;
        _clock = clock;
    }

    public async Task<Result<Unit>> HandleAsync(BeginRecoveryCommand command, CancellationToken ct)
    {
        // 1. Validate email
        var emailResult = Email.Create(command.Email);
        if (emailResult.IsFailure)
        {
            // Don't reveal whether the email is valid
            return Unit.Value;
        }

        Email email = emailResult.Value;

        // 2. Check if user exists and is verified
        var userOption = await _userQueryRepo.FindByEmailAsync(email.Value, ct);
        if (userOption.IsNone)
        {
            // Silent — don't reveal user existence
            return Unit.Value;
        }

        if (!userOption.Value.EmailVerified)
        {
            // Silent — can't recover unverified accounts
            return Unit.Value;
        }

        // 3. Generate recovery code
        DateTimeOffset now = _clock.UtcNow();
        string code = CompleteRegistrationCommandHandler.GenerateCode();
        string codeHash = CompleteRegistrationCommandHandler.HashCode(code);
        var recoveryCodeId = new RecoveryCodeId(_guids.NewGuid());

        var recoveryCodeResult = RecoveryCode.Issue(recoveryCodeId, codeHash, RecoveryCodePurpose.AccountRecovery, now, RecoveryCodeTtl);
        if (recoveryCodeResult.IsFailure)
        {
            return recoveryCodeResult.Error;
        }

        await _recoveryCodeRepo.AddAsync(recoveryCodeResult.Value, ct);
        await _recoveryCodeRepo.SaveChangesAsync(ct);

        // 4. Send email
        await _emailSender.SendRecoveryCodeAsync(email, code, ct);

        return Unit.Value;
    }
}