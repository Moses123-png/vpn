using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WireGuardVpnManager
{
    public partial class MainWindow : Window
    {
        private readonly string tunnelsFolder;
        private readonly WireGuardManager wgManager;
        private readonly FirewallManager fwManager;
        private readonly ProfileManager profileManager;
        private Timer monitorTimer;
        private const int MonitorIntervalMs = 30_000; // 30s
        private string? currentProfileName;

        public MainWindow()
        {
            InitializeComponent();
            tunnelsFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tunnels");
            Directory.CreateDirectory(tunnelsFolder);
            wgManager = new WireGuardManager(tunnelsFolder);
            fwManager = new FirewallManager();
            profileManager = new ProfileManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json"));
            RefreshTunnelList();
            LoadProfiles();
            Log("Ready. Make sure WireGuard for Windows is installed and run this app as Administrator.");
            monitorTimer = new Timer(async (_) => await MonitorTickAsync(), null, MonitorIntervalMs, MonitorIntervalMs);
        }

        private void RefreshTunnelList()
        {
            Dispatcher.Invoke(() =>
            {
                TunnelsList.Items.Clear();
                foreach (var f in Directory.GetFiles(tunnelsFolder, "*.conf"))
                {
                    TunnelsList.Items.Add(System.IO.Path.GetFileName(f));
                }
            });
        }

        private void LoadProfiles()
        {
            profileManager.Load();
        }

        private void ImportBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "WireGuard config (*.conf)|*.conf|All files|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                var dest = Path.Combine(tunnelsFolder, Path.GetFileName(dlg.FileName));
                File.Copy(dlg.FileName, dest, true);
                var name = System.IO.Path.GetFileName(dest);
                if (!profileManager.HasProfileForConfig(name))
                {
                    profileManager.AddOrUpdateProfile(new Profile
                    {
                        ConfigFileName = name,
                        DisplayName = name,
                        AutoConnect = false,
                        AutoStartOnLogin = false
                    });
                    profileManager.Save();
                }
                Log($"Imported {Path.GetFileName(dest)}");
                RefreshTunnelList();
            }
        }

        private async void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = SelectedTunnel();
            if (name == null) { Log("No tunnel selected"); return; }
            try
            {
                var full = Path.Combine(tunnelsFolder, name);
                await wgManager.InstallTunnelAsync(full);
                Log($"Installed/started {name}");
            }
            catch (Exception ex)
            {
                Log($"Error installing tunnel: {ex.Message}");
            }
        }

        private async void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = SelectedTunnel();
            if (name == null) { Log("No tunnel selected"); return; }
            try
            {
                var full = Path.Combine(tunnelsFolder, name);
                await wgManager.UninstallTunnelAsync(full);
                Log($"Uninstalled/stopped {name}");
            }
            catch (Exception ex)
            {
                Log($"Error uninstalling tunnel: {ex.Message}");
            }
        }

        private async void EnableKillBtn_Click(object sender, RoutedEventArgs e)
        {
            var name = SelectedTunnel();
            if (name == null) { Log("No tunnel selected"); return; }
            var full = Path.Combine(tunnelsFolder, name);
            try
            {
                var endpoint = wgManager.ParseEndpoint(full);
                fwManager.AddBlockAllRule();
                var wgExe = wgManager.WireGuardExePath;
                fwManager.AddAllowExeRule("AllowWireGuardExe", wgExe);
                if (endpoint != null)
                {
                    fwManager.AddAllowEndpointRule("AllowWireGuardEndpoint", endpoint.Item1, endpoint.Item2);
                    Log($"Kill switch enabled (allowed endpoint {endpoint.Item1}:{endpoint.Item2})");
                }
                else
                {
                    Log("Kill switch enabled (no endpoint parsed; wireguard.exe allowed only)");
                }
            }
            catch (Exception ex)
            {
                Log($"Error enabling kill switch: {ex.Message}");
            }
        }

        private void DisableKillBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                fwManager.RemoveRule("BlockAllOutbound-VGKM");
                fwManager.RemoveRule("AllowWireGuardExe");
                fwManager.RemoveRule("AllowWireGuardEndpoint");
                Log("Kill switch disabled (firewall rules removed)");
            }
            catch (Exception ex)
            {
                Log($"Error disabling kill switch: {ex.Message}");
            }
        }

        private async void TestBtn_Click(object sender, RoutedEventArgs e)
        {
            Log("Testing public IP...");
            try
            {
                var ip = await Utils.GetPublicIpAsync();
                Log($"Public IP: {ip}");
                var p = CurrentProfile();
                if (p != null)
                {
                    p.LastKnownVpnPublicIp = ip;
                    profileManager.AddOrUpdateProfile(p);
                    profileManager.Save();
                    ProfileLastIpText.Text = $"Last-known VPN IP: {p.LastKnownVpnPublicIp}";
                    Log("Stored current public IP as profile's last-known VPN IP for monitoring.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error testing IP: {ex.Message}");
            }
        }

        private async Task MonitorTickAsync()
        {
            try
            {
                var p = CurrentProfile();
                if (p == null || !p.AutoConnect || string.IsNullOrEmpty(p.LastKnownVpnPublicIp)) return;

                string currentIp;
                try { currentIp = await Utils.GetPublicIpAsync(); }
                catch { return; }

                if (currentIp != p.LastKnownVpnPublicIp)
                {
                    Log($"Monitor: public IP changed ({currentIp} != {p.LastKnownVpnPublicIp}). Attempting reconnect for profile {p.DisplayName}.");
                    var full = Path.Combine(tunnelsFolder, p.ConfigFileName);
                    if (File.Exists(full))
                    {
                        await wgManager.InstallTunnelAsync(full);
                        Log($"Monitor: attempted to start tunnel {p.ConfigFileName}");
                    }
                    else
                    {
                        Log($"Monitor: tunnel config file missing: {p.ConfigFileName}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"Monitor error: {ex.Message}");
            }
        }

        private void AutoConnectCheck_Click(object sender, RoutedEventArgs e)
        {
            var p = CurrentProfile();
            if (p == null) return;
            p.AutoConnect = AutoConnectCheck.IsChecked == true;
            profileManager.AddOrUpdateProfile(p);
            profileManager.Save();
            Log($"Profile '{p.DisplayName}' AutoConnect set to {p.AutoConnect}");
        }

        private void AutoStartCheck_Click(object sender, RoutedEventArgs e)
        {
            var p = CurrentProfile();
            if (p == null) return;
            p.AutoStartOnLogin = AutoStartCheck.IsChecked == true;
            profileManager.AddOrUpdateProfile(p);
            profileManager.Save();

            if (p.AutoStartOnLogin)
            {
                try
                {
                    var exe = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    var args = $"--autostart=\"{p.DisplayName}\"";
                    var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.SetValue("WireGuardVpnManager", $"\"{exe}\" {args}");
                    key?.Close();
                    Log($"Auto-start enabled for profile {p.DisplayName}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to enable auto-start: {ex.Message}");
                }
            }
            else
            {
                try
                {
                    var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true);
                    key?.DeleteValue("WireGuardVpnManager", false);
                    key?.Close();
                    Log($"Auto-start disabled for profile {p.DisplayName}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to disable auto-start: {ex.Message}");
                }
            }
        }

        private void ExportLogsBtn_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
            {
                FileName = $"wg-manager-logs-{DateTime.Now:yyyyMMddHHmmss}.txt",
                Filter = "Text files|*.txt|All files|*.*"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    File.WriteAllText(dlg.FileName, LogBox.Text);
                    Log($"Logs exported to {dlg.FileName}");
                }
                catch (Exception ex)
                {
                    Log($"Failed to export logs: {ex.Message}");
                }
            }
        }

        private string? SelectedTunnel()
        {
            return TunnelsList.SelectedItem as string;
        }

        private void TunnelsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            currentProfileName = SelectedTunnel();
            var p = CurrentProfile();
            if (p != null)
            {
                ProfileNameText.Text = $"Profile: {p.DisplayName}";
                ProfileFileText.Text = $"Config: {p.ConfigFileName}";
                ProfileLastIpText.Text = string.IsNullOrEmpty(p.LastKnownVpnPublicIp) ? "Last-known VPN IP: (none)" : $"Last-known VPN IP: {p.LastKnownVpnPublicIp}";
                AutoConnectCheck.IsChecked = p.AutoConnect;
                AutoStartCheck.IsChecked = p.AutoStartOnLogin;
            }
            else
            {
                ProfileNameText.Text = "(no profile selected)";
                ProfileFileText.Text = "";
                ProfileLastIpText.Text = "";
                AutoConnectCheck.IsChecked = false;
                AutoStartCheck.IsChecked = false;
            }
        }

        private Profile? CurrentProfile()
        {
            var sel = SelectedTunnel();
            if (sel == null) return null;
            return profileManager.GetProfileForConfig(sel);
        }

        public async Task TryAutoStartProfileAsync(string profileDisplayName)
        {
            try
            {
                var p = profileManager.GetProfileByDisplayName(profileDisplayName);
                if (p == null) return;
                var full = Path.Combine(tunnelsFolder, p.ConfigFileName);
                if (File.Exists(full))
                {
                    await wgManager.InstallTunnelAsync(full);
                    Log($"Auto-start: installed/started tunnel {p.ConfigFileName}");
                }
            }
            catch (Exception ex)
            {
                Log($"Auto-start error: {ex.Message}");
            }
        }

        private void Log(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText($"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                LogBox.ScrollToEnd();
            });
        }
    }
}
