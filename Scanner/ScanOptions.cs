namespace SharpScan.Scanner;

sealed class ScanOptions
{
    public required IEnumerable<string> Targets { get; init; }
    public required IEnumerable<int> Ports { get; init; }
    public int TimeoutMs { get; init; } = 1000;
    public int MaxConcurrency { get; init; } = 1000;
}
