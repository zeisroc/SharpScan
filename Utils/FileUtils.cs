namespace SharpScan.Utils;

static class FileUtils
{
    public static IEnumerable<string> ReadLines(string path, CancellationToken ct = default)
    {
        if (!File.Exists(path)) yield break;

        foreach (var line in File.ReadLines(path))
        {
            ct.ThrowIfCancellationRequested();

            var trimmed = line.Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith('#')) continue;
            yield return trimmed;
        }
    }
}
