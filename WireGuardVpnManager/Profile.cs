namespace WireGuardVpnManager
{
    public class Profile
    {
        public string DisplayName { get; set; } = "";
        public string ConfigFileName { get; set; } = "";
        public bool AutoConnect { get; set; } = false;
        public bool AutoStartOnLogin { get; set; } = false;
        public string? LastKnownVpnPublicIp { get; set; }
    }
}
