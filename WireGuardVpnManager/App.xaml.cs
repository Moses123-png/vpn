using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace WireGuardVpnManager
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            string? autoProfile = null;
            foreach (var arg in Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("--autostart=", StringComparison.OrdinalIgnoreCase))
                {
                    autoProfile = arg.Substring("--autostart=".Length).Trim('"');
                }
            }

            var main = new MainWindow();

            if (!string.IsNullOrEmpty(autoProfile))
            {
                Task.Run(async () =>
                {
                    await Task.Delay(1500);
                    await main.TryAutoStartProfileAsync(autoProfile);
                });
            }

            main.Show();
        }
    }
}
