using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Queries;
using Gestalt.Lib.Primitives;

namespace Gestalt.Lib.Application.Mediator;

public interface ISender
{
    Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken);

    Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken);
}