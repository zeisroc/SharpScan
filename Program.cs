using System.Collections.Concurrent;
using SharpScan.Scanner;

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

// --- Argument parsing ---
string? target = null;
string? portsArg = null;
int timeoutMs = 1000;
int concurrency = 1000;

for (var i = 0; i < args.Length; i++)
{
    switch (args[i])
    {
        case "-p" or "--ports":
            if (i + 1 < args.Length) portsArg = args[++i];
            break;
        case "-t" or "--timeout":
            if (i + 1 < args.Length) int.TryParse(args[++i], out timeoutMs);
            break;
        case "-c" or "--concurrency":
            if (i + 1 < args.Length) int.TryParse(args[++i], out concurrency);
            break;
        default:
            if (!args[i].StartsWith('-')) target = args[i];
            break;
    }
}

if (target is null)
{
    Console.Error.WriteLine("Usage: sharpscan <target[,target...]> [-p <ports>] [-t <timeout_ms>] [-c <concurrency>]");
    Console.Error.WriteLine("  target   IP, CIDR (e.g. 192.168.1.0/24), file path, or comma-separated mix");
    Console.Error.WriteLine("  -p       Ports: single (80), list (80,443), range (1-1024). Default: built-in list");
    Console.Error.WriteLine("  -t       Timeout in ms (default 1000)");
    Console.Error.WriteLine("  -c       Max concurrency (default 1000)");
    return 1;
}

IEnumerable<int> ports = portsArg is not null
    ? PortParser.Parse(portsArg)
    : PortParser.Parse(DefaultPorts.List);

var options = new ScanOptions
{
    Targets = target.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries),
    Ports = ports,
    TimeoutMs = timeoutMs,
    MaxConcurrency = concurrency
};

var sourceHostname = System.Net.Dns.GetHostName();
var sourceAddresses = System.Net.Dns.GetHostAddresses(sourceHostname);
var sourceIp = sourceAddresses
    .FirstOrDefault(a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    ?.ToString() ?? "unknown";
Console.WriteLine($"# {sourceIp} / {sourceHostname}");

var results = new ConcurrentDictionary<string, ConcurrentBag<int>>();

var engine = new ScanEngine();
await engine.RunAsync(options, (ip, port) =>
{
    results.GetOrAdd(ip, _ => new ConcurrentBag<int>()).Add(port);
}, cts.Token);

if (cts.IsCancellationRequested)
    return 130;

foreach (var (ip, ports2) in results.OrderBy(kv => kv.Key, StringComparer.Ordinal))
{
    var sorted = ports2.Order();
    Console.WriteLine($"{ip}:{string.Join(",", sorted)}");
}

return 0;

// -----------------------------------------------------------------------
// Default port list (security-relevant ports commonly found in pen-tests)
// -----------------------------------------------------------------------
static class DefaultPorts
{
    public const string List =
        "21,22,23,25,49,53,79,80,81,82,83,84,85,86,87,88,89,90," +
        "111,137,138,139,311,379,389,390,443,445,446,443,548,512,513,514," +
        "623,631,636,873,902,990," +
        "1090,1098,1099,1194,1234,1241,1352,1414,1433,1443,1494,1521,1522,1523," +
        "1524,1525,1526,1527,1528,1529,1530,1583,1723,1812,1883," +
        "2000,2006,2049,2082,2083,2086,2100,2106,2121,2156,2205,2222,2224," +
        "2301,2376,2381,2638,3000,3050,3128,3200,3260,3268,3269,3299,3306," +
        "3310,3351,3389,3466,3480,3632,3668,3690,4001,4280,4343,4443,4444," +
        "4445,4679,4711,4743,4750,4786,4848,4949,5000,5001,5005,5006,5102," +
        "5353,5432,5433,5445,5555,5556,5601,5666,5672,5800,5869,5900,5901," +
        "5902,5903,5938,5984,5985,5986,6000,6001,6002,6003,6004,6005,6006," +
        "6007,6008,6009,6060,6061,6080,6112,6129,6161,6167,6262,6379,6690," +
        "6969,7000,7001,7002,7003,7004,7070,7071,7080,7183,7272,7474,7670," +
        "7676,7779,8000,8001,8002,8003,8004,8005,8006,8007,8008,8009,8010," +
        "8080,8081,8082,8083,8084,8085,8086,8087,8088,8089,8090,8091,8092," +
        "8093,8094,8095,8096,8097,8098,8099,8100,8180,8181,8182,8183,8184," +
        "8200,8291,8300,8380,8381,8382,8383,8384,8385,8386,8387,8388,8389," +
        "8390,8400,8443,8444,8445,8480,8500,8530,8531,8600,8700,8800,8880," +
        "8888,8889,8899,8900,8983,8989,8999,9000,9001,9002,9003,9004,9005," +
        "9006,9007,9008,9009,9010,9042,9043,9050,9080,9090,9091,9092,9093," +
        "9094,9095,9096,9097,9098,9099,9100,9200,9201,9300,9389,9390,9443," +
        "9500,9600,9700,9800,9900,9999,10000,10443,10999,11000,11099,11211," +
        "12174,12345,13131,13785,16102,16200,17988,17990,19200,27017,27018," +
        "27019,27080,28017,32764,35357,38292,41080,41443,45000,45001,47001," +
        "47002,50000,50001,50002,50003,50004,50005,50006,50007,50008,50009," +
        "50010,50013,50070,50080,50090,50100,50107,50500";
}
