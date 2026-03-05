using System.Net;

namespace SharpScan.Utils;

static class IpUtils
{
    public static bool TryParse(string input, out IPAddress address) =>
        IPAddress.TryParse(input, out address!);

    public static bool IsValid(string input) =>
        IPAddress.TryParse(input, out _);
}
