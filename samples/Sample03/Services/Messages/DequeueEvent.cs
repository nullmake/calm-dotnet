using Calm.Core;

namespace Sample03.Services.Messages;

internal sealed record DequeueEvent(int Number, int Remains) : ICalmEvent
{
}
