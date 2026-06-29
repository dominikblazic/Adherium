namespace Adherium.Adherence.Core.Results;

public sealed record BatchSummary
{
    public required int Received { get; init; }
    public required int Processed { get; init; }
    public required int Duplicates { get; init; }
    public required int Unattributed { get; init; }
}
