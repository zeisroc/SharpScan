# SharpScan

A high-performance, cross-platform TCP port scanner written in C# (.NET 8).  
Ships as a **single self-contained executable** - no runtime installation required.

---

## Features

- **Single self-contained binary** - drop it anywhere and run it; no .NET runtime needed on the target machine
- **TCP connect scan** - fast, reliable, fully async
- **Flexible target input**
  - Single IP: `192.168.1.10`
  - Multiple IPs: `192.168.1.10,192.168.1.11,192.168.1.12`
  - CIDR range: `192.168.1.0/24` (memory-efficient, streamed via `yield`)
  - Comma-separated mix: `10.0.0.1,192.168.1.0/24,172.16.0.5`
  - File with mixed targets (IPs and CIDRs, one per line, `#` comments supported)
- **Flexible port input**
  - Single port: `80`
  - Comma list: `22,80,443,8080`
  - Range: `1-1024`
  - Mixed: `22,80,443,8000-8100`
  - Omit `-p` to use the built-in security-relevant port list (~400 ports)
- **Configurable concurrency** - default 1000 simultaneous connections
- **Configurable timeout** - per-connection timeout in milliseconds (default 1000 ms)
- **Clean output** - strictly `ip:port`, one result per line, nothing else
- **Graceful cancellation** - `Ctrl+C` stops the scan cleanly
- **Cross-platform** - runs natively on Linux and Windows (x64)

---

## Quickstart

### Download a pre-built binary

Grab the latest release for your platform from the [Releases](../../releases) page.

```bash
# Linux
chmod +x sharpscan
./sharpscan 192.168.1.1 -p 22,80,443

# Windows
sharpscan.exe 192.168.1.1 -p 22,80,443
```

### Build from source

Requirements: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
git clone <repo-url>
cd SharpScan

# Run directly
make run ARGS="192.168.1.1 -p 22,80,443"

# Build a single self-contained binary
make publish-linux     # -> dist/linux/sharpscan
make publish-windows   # -> dist/windows/sharpscan.exe
make publish           # both platforms at once
```

---

## Usage

```
sharpscan <target[,target...]> [-p <ports>] [-t <timeout_ms>] [-c <concurrency>]
```

| Argument | Description | Default |
|---|---|---|
| `target` | IP, CIDR, file path, or **comma-separated mix** of any of those | *(required)* |
| `-p, --ports` | Port(s) to scan | Built-in list |
| `-t, --timeout` | Connection timeout in milliseconds | `1000` |
| `-c, --concurrency` | Maximum simultaneous connections | `1000` |

---

## Examples

**Scan multiple specific hosts:**
```bash
sharpscan 192.168.1.10,192.168.1.20,192.168.1.30 -p 22,80,443
```

**Mix IPs, CIDRs, and a file in one command:**
```bash
sharpscan 10.0.0.1,192.168.1.0/24,targets.txt -p 80,443
```

**Scan a single host for common web ports:**
```bash
sharpscan 10.0.0.1 -p 80,443,8080,8443
```

**Scan a /24 subnet for SSH:**
```bash
sharpscan 192.168.1.0/24 -p 22
```

**Scan a /24 with a port range, higher concurrency and lower timeout:**
```bash
sharpscan 192.168.1.0/24 -p 1-1024 -c 2000 -t 500
```

**Scan from a target file:**
```bash
# targets.txt
# 192.168.1.10
# 10.0.0.0/24
# 172.16.1.5

sharpscan targets.txt -p 22,80,443
```

**Use the built-in port list (security-relevant ~400 ports):**
```bash
sharpscan 10.0.0.1
```

**Pipe results into a file:**
```bash
sharpscan 192.168.1.0/24 -p 80,443 > open_ports.txt
```

**Chain with grep:**
```bash
sharpscan 192.168.1.0/24 -p 22,3389 | grep ':3389'
```

---

## Output Format

Results are grouped by IP, with open ports listed in ascending order, one host per line:

```
ip: port1, port2, port3
```

Example:
```
192.168.1.10: 22, 80, 443
10.0.0.5: 21, 80, 8080, 8443
```

No banners, no colour, no timestamps - designed for easy piping and parsing.

---

## Target File Format

Lines are trimmed. Blank lines and lines starting with `#` are ignored.

```
# Web servers
192.168.1.10
192.168.1.11

# Internal subnet
10.0.0.0/24

172.16.1.5
```

---

## Building with Make

```
make build           # Debug build (dotnet build)
make release         # Release build
make run ARGS="..."  # Run directly via dotnet run
make publish-linux   # Single binary -> dist/linux/sharpscan
make publish-windows # Single binary -> dist/windows/sharpscan.exe
make publish         # Both platforms
make clean           # Remove bin/ obj/ dist/
```

---

## Cross-Compilation

SharpScan is developed on Linux and cross-compiles to Windows without any Windows-specific dependencies or P/Invoke.

```bash
# On Linux, build the Windows binary:
make publish-windows
# Output: dist/windows/sharpscan.exe  (single file, no .NET runtime needed on target)
```

---

## Performance Notes

- Uses `SemaphoreSlim` to bound concurrent connections (tune with `-c`)
- IPs are enumerated lazily via `yield` - scanning a `/8` won't exhaust memory
- Results are printed immediately as ports are found (streamed, not buffered)
- Lower `-t` (timeout) significantly increases scan speed on responsive networks

---

## Limitations

- TCP connect scan only (no SYN scan / raw packets)
- IPv4 only
- No banner grabbing or service detection (planned for a future release)
