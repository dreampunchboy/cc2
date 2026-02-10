using System.Net;
using System.Text.RegularExpressions;

var hostname = args.Length > 0 ? args[0].Trim() : "CC2";
if (string.IsNullOrEmpty(hostname))
{
    Console.WriteLine("Usage: HostSetup.exe [hostname]");
    return 1;
}

// Normalize: only allow safe hostname characters
if (!Regex.IsMatch(hostname, @"^[a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9]$|^[a-zA-Z0-9]$"))
{
    Console.WriteLine("Invalid hostname. Use letters, numbers, and hyphens only.");
    return 1;
}

var hostsPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.System),
    "drivers", "etc", "hosts");

if (!File.Exists(hostsPath))
{
    Console.WriteLine($"Hosts file not found: {hostsPath}");
    return 1;
}

var entry = $"127.0.0.1 {hostname}";
var lines = File.ReadAllLines(hostsPath);
var entryExists = lines.Any(line =>
{
    var trimmed = line.Trim();
    if (trimmed.Length == 0 || trimmed.StartsWith('#')) return false;
    var parts = trimmed.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
    return parts.Length >= 2 && parts[0] == "127.0.0.1" &&
           string.Equals(parts[1], hostname, StringComparison.OrdinalIgnoreCase);
});

if (entryExists)
{
    Console.WriteLine($"Hosts entry for '{hostname}' already present.");
    return 0;
}

try
{
    File.AppendAllText(hostsPath, Environment.NewLine + entry + Environment.NewLine);
    Console.WriteLine($"Added: {entry}");
    return 0;
}
catch (UnauthorizedAccessException)
{
    Console.WriteLine("Run this program as Administrator to modify the hosts file.");
    return 1;
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    return 1;
}
