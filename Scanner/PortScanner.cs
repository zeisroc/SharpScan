using System.Net;
using System.Net.Sockets;

namespace SharpScan.Scanner;

static class PortScanner
{
    public static async Task<bool> ScanAsync(IPAddress ip, int port, int timeoutMs, CancellationToken ct)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Blocking = false;

        try
        {
            var connectTask = socket.ConnectAsync(ip, port, ct).AsTask();
            var timeoutTask = Task.Delay(timeoutMs, ct);
            var completed = await Task.WhenAny(connectTask, timeoutTask).ConfigureAwait(false);

            return completed == connectTask && socket.Connected;
        }
        catch
        {
            return false;
        }
    }
}
