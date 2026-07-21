using System.Net;

namespace SharpScan.Utils;

static class CidrUtils
{
    public static IEnumerable<IPAddress> Enumerate(string cidr, CancellationToken ct = default)
    {
        var slash = cidr.IndexOf('/');
        if (slash < 0) yield break;

        if (!IPAddress.TryParse(cidr.AsSpan(0, slash), out var baseIp)) yield break;
        if (!int.TryParse(cidr.AsSpan(slash + 1), out var prefix) || prefix < 0 || prefix > 32) yield break;

        var bytes = baseIp.GetAddressBytes();
        uint ipInt = ((uint)bytes[0] << 24) | ((uint)bytes[1] << 16) | ((uint)bytes[2] << 8) | bytes[3];

        uint mask = prefix == 0 ? 0u : (~0u << (32 - prefix));
        uint network = ipInt & mask;
        uint broadcast = network | ~mask;

        // exclude network address and broadcast address
        uint first = network + 1;
        uint last = broadcast - 1;

        if (first > last) yield break;

        for (uint ip = first; ip <= last; ip++)
        {
            ct.ThrowIfCancellationRequested();

            yield return new IPAddress(new byte[]
            {
                (byte)(ip >> 24),
                (byte)(ip >> 16),
                (byte)(ip >> 8),
                (byte)ip
            });
        }
    }
}
