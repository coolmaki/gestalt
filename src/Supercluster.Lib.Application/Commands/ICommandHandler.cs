using Supercluster.Lib.Primitives;

namespace Supercluster.Lib.Application.Commands;

public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    Task<Result<TResult>> HandleAsync(TCommand command, CancellationToken cancellationToken);
}
