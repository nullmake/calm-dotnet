using Calm.Core;

namespace Sample03.Services.Messages;

internal sealed record AddCommand(params int[] Numbers) : ICalmCommand
{
}
