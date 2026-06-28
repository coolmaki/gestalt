using Passport.Core.Application.ReadModels;

namespace Passport.Core.Application.Queries;

public sealed record GetCredentialsResult(IReadOnlyList<CredentialInfo> Credentials);