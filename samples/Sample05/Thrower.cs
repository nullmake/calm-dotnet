using Calm.Core;
using Microsoft.Extensions.Logging;
using SharedLibrary;

namespace Sample05;

internal sealed class Thrower
{
    private readonly ILogger _logger = ConsoleLogger.Create<Thrower>();

    private void PutSeparator()
        => _logger.LogInformation("{Separator}", new string('-', 32));

    private static string CannotCatchMessage(string name)
        => $"The user cannot catch the exception thrown by {name}; the error observer can, however.";

    public async Task ExecuteAsync(ICalm calm)
    {
        _logger.LogInformation("");
        _logger.LogInformation("*****************************************");
        _logger.LogInformation(" ICalmErrorObserver.OnUnhandledException");
        _logger.LogInformation("*****************************************");

        await CallAsyncMethodsThatCanCatchExceptions(calm).ConfigureAwait(false);
        await CallSyncMethodsThatCanCatchExceptions(calm).ConfigureAwait(false);
        await CallAsyncMethodsThatCannotCatchExceptions(calm).ConfigureAwait(false);
        await CallSyncMethodsThatCannotCatchExceptions(calm).ConfigureAwait(false);
        PutSeparator();
    }

    private async Task CallAsyncMethodsThatCanCatchExceptions(ICalm calm)
    {
        foreach (var item in new (string Name, Func<Task> Func)[]
        {
            (
                "ExecuteAsync",
                () => calm.ExecuteAsync(_ => throw new InvalidOperationException())
            ),
            (
                "Command.SendAsync",
                () => calm.Command.SendAsync(new TestCommand())
            ),
            (
                "Command.SendAsync<>",
                () => calm.Command.SendAsync(new TestCommandWithResponse())
            ),
            (
                "Query.SendAsync",
                () => calm.Query.SendAsync(new TestQuery())
            ),
        })
        {
            try
            {
                PutSeparator();
                _logger.LogInformation("Call `{Name}`.", item.Name);

                await item.Func().ConfigureAwait(false);

                throw new NotSupportedException("This error should not occur.");
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("The user must catch the exception thrown by {Name}", item.Name);
            }
        }
    }

    private async Task CallSyncMethodsThatCanCatchExceptions(ICalm calm)
    {
        foreach (var item in new (string Name, Action Action)[]
        {
            (
                "ExecuteAsync + .GetAwaiter().GetResult()",
                () => calm.ExecuteAsync(_ => throw new InvalidOperationException()).GetAwaiter().GetResult()
            ),
            (
                "Command.SendAsync + .GetAwaiter().GetResult()",
                () => calm.Command.SendAsync(new TestCommand()).GetAwaiter().GetResult()
            ),
            (
                "Command.SendAsync<> + .GetAwaiter().GetResult()",
                () => calm.Command.SendAsync(new TestCommandWithResponse()).GetAwaiter().GetResult()
            ),
            (
                "Query.SendAsync + .GetAwaiter().GetResult()",
                () => calm.Query.SendAsync(new TestQuery()).GetAwaiter().GetResult()
            ),
        })
        {
            try
            {
                PutSeparator();
                _logger.LogInformation("Call `{Name}`.", item.Name);

                item.Action();

                throw new NotSupportedException("This error should not occur.");
            }
            catch (InvalidOperationException)
            {
                _logger.LogError("The user must catch the exception thrown by {Name}", item.Name);
            }
        }
    }

    private async Task CallAsyncMethodsThatCannotCatchExceptions(ICalm calm)
    {
        foreach (var item in new (string Name, Func<string, Task> Func)[]
        {
            (
                "ScheduleAsync",
                name => calm.ScheduleAsync(_ => throw new InvalidOperationException(CannotCatchMessage(name)))
            ),
            (
                "Command.PostAsync",
                name => calm.Command.PostAsync(new TestCommand(CannotCatchMessage(name)))
            ),
            (
                "Command.PostAsync<>",
                name => calm.Command.PostAsync(new TestCommandWithResponse(CannotCatchMessage(name)))
            ),
            (
                "Command.PublishAsync",
                name => calm.Event.PublishAsync(new TestEvent(CannotCatchMessage(name)))
            ),
        })
        {
            try
            {
                PutSeparator();
                _logger.LogInformation("Call `{Name}`.", item.Name);

                await item.Func(item.Name).ConfigureAwait(false);
                await calm.ExecuteAsync(_ => Task.CompletedTask);
            }
            catch
            {
                throw new NotSupportedException("This error should not occur.");
            }
        }
    }

    private async Task CallSyncMethodsThatCannotCatchExceptions(ICalm calm)
    {
        foreach (var item in new (string Name, Action<string> Action)[]
        {
            (
                "ScheduleAsync + .GetAwaiter().GetResult()",
                name => calm.ScheduleAsync(_ => throw new InvalidOperationException(CannotCatchMessage(name)))
                    .GetAwaiter().GetResult()
            ),
            (
                "Schedule",
                name => calm.Schedule(_ => throw new InvalidOperationException(CannotCatchMessage(name)))
            ),
            (
                "Command.Post",
                name => calm.Command.Post(new TestCommand(CannotCatchMessage(name)))
            ),
            (
                "Command.PostAsync + .GetAwaiter().GetResult()",
                name => calm.Command.PostAsync(new TestCommand(CannotCatchMessage(name)))
                    .GetAwaiter().GetResult()
            ),
            (
                "Command.PostAsync<> + .GetAwaiter().GetResult()",
                name => calm.Command.PostAsync(new TestCommandWithResponse(CannotCatchMessage(name)))
                    .GetAwaiter().GetResult()
            ),
            (
                "Command.Publish",
                name => calm.Event.Publish(new TestEvent(CannotCatchMessage(name)))
            ),
        })
        {
            try
            {
                PutSeparator();
                _logger.LogInformation("Call `{Name}`.", item.Name);

                item.Action(item.Name);
                await calm.ExecuteAsync(_ => Task.CompletedTask);
            }
            catch
            {
                throw new NotSupportedException("This error should not occur.");
            }
        }
    }

    [CalmHandler]
    private static Task HandleTestCommandAsync(TestCommand command, CancellationToken token)
        => throw new InvalidOperationException(command.Message);

    [CalmHandler]
    private static Task<bool> HandleTestCommandWithResponseAsync(TestCommandWithResponse command, CancellationToken token)
        => throw new InvalidOperationException(command.Message);

    [CalmHandler]
    private static Task HandleTestEventAsync(TestEvent @event, CancellationToken token)
       => throw new InvalidOperationException(@event.Message);

    [CalmHandler]
    private static Task<bool> HandleTestQueryAsync(TestQuery query, CancellationToken token)
        => throw new InvalidOperationException(query.Message);
}

#region Messages
internal sealed record TestCommand(string Message = "") : ICalmCommand;
internal sealed record TestCommandWithResponse(string Message = "") : ICalmCommand<bool>;
internal sealed record TestEvent(string Message = "") : ICalmEvent;
internal sealed record TestQuery(string Message = "") : ICalmQuery<bool>;
#endregion
