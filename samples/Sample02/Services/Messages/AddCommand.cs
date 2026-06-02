using Calm.Core;
using Sample02;

namespace Sample02.Services.Messages;

internal sealed record AddCommand(params int[] Numbers) : ICalmCommand
{
}
