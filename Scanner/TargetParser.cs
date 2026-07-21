using System.Net;
using SharpScan.Utils;

namespace SharpScan.Scanner;

static class TargetParser
{
    public static IEnumerable<IPAddress> Parse(string input, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        if (File.Exists(input))
        {
            foreach (var line in FileUtils.ReadLines(input, ct))
            foreach (var ip in Parse(line, ct))
            {
                ct.ThrowIfCancellationRequested();
                yield return ip;
            }
        }
        else if (input.Contains('/'))
        {
            foreach (var ip in CidrUtils.Enumerate(input, ct))
            {
                ct.ThrowIfCancellationRequested();
                yield return ip;
            }
        }
        else if (IpUtils.TryParse(input, out var address))
        {
            ct.ThrowIfCancellationRequested();
            yield return address;
        }
    }
}
