using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WireGuardVpnManager
{
    public class WireGuardManager
    {
        private readonly string tunnelsFolder;
        public string WireGuardExePath { get; }

        public WireGuardManager(string tunnelsFolder)
        {
            this.tunnelsFolder = tunnelsFolder;
            var candidate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WireGuard", "wireguard.exe");
            if (File.Exists(candidate)) WireGuardExePath = candidate;
            else WireGuardExePath = "wireguard.exe";
        }

        public Task InstallTunnelAsync(string confFullPath)
        {
            return RunWireGuardCommandAsync($"/installtunnelservice \"{confFullPath}\"");
        }

        public Task UninstallTunnelAsync(string confFullPath)
        {
            return RunWireGuardCommandAsync($"/uninstalltunnelservice \"{confFullPath}\"");
        }

        private Task RunWireGuardCommandAsync(string args)
        {
            var tcs = new TaskCompletionSource<object?>();
            var psi = new ProcessStartInfo
            {
                FileName = WireGuardExePath,
                Arguments = args,
                UseShellExecute = true,
                Verb = "runas",
                WindowStyle = ProcessWindowStyle.Hidden
            };
            try
            {
                var p = Process.Start(psi);
                if (p == null) throw new InvalidOperationException("Could not start wireguard.exe. Is WireGuard installed and the executable available?");
                p.EnableRaisingEvents = true;
                p.Exited += (s, e) =>
                {
                    tcs.SetResult(null);
                    p.Dispose();
                };
            }
            catch (Exception ex)
            {
                tcs.SetException(ex);
            }
            return tcs.Task;
        }

        public Tuple<string, int>? ParseEndpoint(string confFullPath)
        {
            if (!File.Exists(confFullPath)) return null;
            var text = File.ReadAllText(confFullPath);
            var m = Regex.Match(text, @"Endpoint\s*=\s*([^\s:]+):(\d+)", RegexOptions.IgnoreCase);
            if (!m.Success) return null;
            var host = m.Groups[1].Value.Trim();
            var port = int.Parse(m.Groups[2].Value);

            if (IPAddress.TryParse(host, out _))
            {
                return Tuple.Create(host, port);
            }
            try
            {
                var ips = Dns.GetHostAddresses(host);
                foreach (var ip in ips)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return Tuple.Create(ip.ToString(), port);
                    }
                }
                if (ips.Length > 0) return Tuple.Create(ips[0].ToString(), port);
            }
            catch
            {
            }
            return Tuple.Create(host, port);
        }
    }
}
