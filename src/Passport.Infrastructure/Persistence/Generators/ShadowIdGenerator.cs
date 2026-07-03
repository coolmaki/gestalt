using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ValueGeneration;

namespace Passport.Infrastructure.Persistence.Generators;

/// <summary>
/// Generates client-side GUIDs for shadow primary keys.
/// <see cref="GeneratesTemporaryValues"/> is <c>false</c> so the value
/// is treated as permanent (not a temp key that gets replaced by the DB).
/// </summary>
internal sealed class ShadowIdGenerator : ValueGenerator<Guid>
{
    public override Guid Next(EntityEntry entry) => Guid.CreateVersion7();

    public override bool GeneratesTemporaryValues => false;
}