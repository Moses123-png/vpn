using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace WireGuardVpnManager
{
    public static class Utils
    {
        private static readonly HttpClient http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };

        public static async Task<string> GetPublicIpAsync()
        {
            var resp = await http.GetStringAsync("https://api.ipify.org?format=json");
            using var doc = JsonDocument.Parse(resp);
            if (doc.RootElement.TryGetProperty("ip", out var el))
            {
                return el.GetString() ?? "";
            }
            return "";
        }
    }
}
