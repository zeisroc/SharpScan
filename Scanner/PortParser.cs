namespace SharpScan.Scanner;

static class PortParser
{
    public static IEnumerable<int> Parse(string input)
    {
        foreach (var token in input.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var dashIdx = token.IndexOf('-');
            if (dashIdx > 0)
            {
                if (int.TryParse(token.AsSpan(0, dashIdx), out var start) &&
                    int.TryParse(token.AsSpan(dashIdx + 1), out var end) &&
                    IsValid(start) && IsValid(end) && start <= end)
                {
                    for (var p = start; p <= end; p++)
                        yield return p;
                }
            }
            else if (int.TryParse(token, out var port) && IsValid(port))
            {
                yield return port;
            }
        }
    }

    static bool IsValid(int port) => port >= 1 && port <= 65535;
}
