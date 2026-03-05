using System.Net;
using SharpScan.Utils;

namespace SharpScan.Scanner;

static class TargetParser
{
    public static IEnumerable<IPAddress> Parse(string input)
    {
        if (File.Exists(input))
        {
            foreach (var line in FileUtils.ReadLines(input))
            foreach (var ip in Parse(line))
                yield return ip;
        }
        else if (input.Contains('/'))
        {
            foreach (var ip in CidrUtils.Enumerate(input))
                yield return ip;
        }
        else if (IpUtils.TryParse(input, out var address))
        {
            yield return address;
        }
    }
}
