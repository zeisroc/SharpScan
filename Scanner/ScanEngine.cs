using System.Net;

namespace SharpScan.Scanner;

sealed class ScanEngine
{
    public async Task RunAsync(ScanOptions options, Action<string, int> onResult, CancellationToken ct)
    {
        using var semaphore = new SemaphoreSlim(options.MaxConcurrency, options.MaxConcurrency);

        // Materialise ports once; they may be iterated many times (one per target)
        var ports = options.Ports is ICollection<int> col ? col : options.Ports.ToArray();

        foreach (var target in options.Targets)
        {
            if (ct.IsCancellationRequested) break;

            IEnumerable<IPAddress> addresses;
            try { addresses = TargetParser.Parse(target); }
            catch { continue; }

            foreach (var ip in addresses)
            {
                if (ct.IsCancellationRequested) break;

                foreach (var port in ports)
                {
                    if (ct.IsCancellationRequested) break;

                    await semaphore.WaitAsync(ct).ConfigureAwait(false);

                    var capturedIp = ip;
                    var capturedPort = port;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var open = await PortScanner.ScanAsync(capturedIp, capturedPort, options.TimeoutMs, ct)
                                .ConfigureAwait(false);
                            if (open)
                                onResult(capturedIp.ToString(), capturedPort);
                        }
                        catch { /* silently ignore */ }
                        finally
                        {
                            semaphore.Release();
                        }
                    }, ct);
                }
            }
        }

        // Drain: wait until all in-flight tasks have released the semaphore
        for (var i = 0; i < options.MaxConcurrency; i++)
            await semaphore.WaitAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
