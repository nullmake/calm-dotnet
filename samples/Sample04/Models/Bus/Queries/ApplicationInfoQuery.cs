using Calm.Core;
using System.Diagnostics.CodeAnalysis;

namespace Sample04.Models.Bus.Queries;

internal sealed record ApplicationInfoQuery : ICalmQuery<ApplicationInfoQueryResult>
{
}

internal sealed record ApplicationInfoQueryResult
{
    public required string Name { get; init; }
    public required string Version { get; init; }

    public ApplicationInfoQueryResult()
    {
    }

    [SetsRequiredMembers]
    public ApplicationInfoQueryResult(string name, string version)
    {
        Name = name;
        Version = version;
    }
}
