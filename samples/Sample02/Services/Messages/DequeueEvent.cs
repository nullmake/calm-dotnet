using Calm.Core;

namespace Sample02.Services.Messages;

internal sealed record DequeueEvent(int Number, int Remains) : ICalmEvent
{
}
