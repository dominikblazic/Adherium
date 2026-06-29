namespace Adherium.Adherence.Core.Results;

/// <summary>Headline counts for the batch.</summary>
public sealed record BatchSummary
{
    public required int Received { get; init; }
    public required int Processed { get; init; }
    public required int Duplicates { get; init; }
    public required int Unattributed { get; init; }
}
