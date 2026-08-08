using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Gestalt.Lib.Application.Commands;
using Gestalt.Lib.Application.Queries;
using Gestalt.Lib.Primitives;

namespace Gestalt.Lib.Application.Mediator;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, (Type HandlerType, MethodInfo Method)> CommandCache = new();
    private static readonly ConcurrentDictionary<Type, (Type HandlerType, MethodInfo Method)> QueryCache = new();

    public async Task<Result<TResult>> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken)
    {
        var (handlerType, method) = CommandCache.GetOrAdd(command.GetType(), ct =>
        {
            var handlerInterface = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
            var handlerType2 = handlerInterface;
            var methodInfo = handlerInterface.GetMethod("HandleAsync")!;
            return (handlerType2, methodInfo);
        });

        var handler = serviceProvider.GetRequiredService(handlerType);
        var task = (Task<Result<TResult>>)method.Invoke(handler, [command, cancellationToken])!;
        return await task;
    }

    public async Task<Result<TResult>> SendAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken)
    {
        var (handlerType, method) = QueryCache.GetOrAdd(query.GetType(), qt =>
        {
            var handlerInterface = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));
            var methodInfo = handlerInterface.GetMethod("HandleAsync")!;
            return (handlerInterface, methodInfo);
        });

        var handler = serviceProvider.GetRequiredService(handlerType);
        var task = (Task<Result<TResult>>)method.Invoke(handler, [query, cancellationToken])!;
        return await task;
    }
}