using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace WireGuardVpnManager
{
    public class ProfileManager
    {
        private readonly string jsonPath;
        private List<Profile> profiles = new();

        public ProfileManager(string jsonPath)
        {
            this.jsonPath = jsonPath;
        }

        public void Load()
        {
            if (!File.Exists(jsonPath))
            {
                profiles = new List<Profile>();
                Save();
                return;
            }
            var txt = File.ReadAllText(jsonPath);
            try
            {
                profiles = JsonSerializer.Deserialize<List<Profile>>(txt) ?? new List<Profile>();
            }
            catch
            {
                profiles = new List<Profile>();
            }
        }

        public void Save()
        {
            var txt = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(jsonPath, txt);
        }

        public Profile? GetProfileForConfig(string configFileName)
        {
            return profiles.FirstOrDefault(p => string.Equals(p.ConfigFileName, configFileName, StringComparison.OrdinalIgnoreCase));
        }

        public bool HasProfileForConfig(string configFileName)
        {
            return GetProfileForConfig(configFileName) != null;
        }

        public void AddOrUpdateProfile(Profile p)
        {
            var existing = GetProfileForConfig(p.ConfigFileName);
            if (existing != null)
            {
                existing.DisplayName = p.DisplayName;
                existing.AutoConnect = p.AutoConnect;
                existing.AutoStartOnLogin = p.AutoStartOnLogin;
                existing.LastKnownVpnPublicIp = p.LastKnownVpnPublicIp;
            }
            else
            {
                profiles.Add(p);
            }
        }

        public Profile? GetProfileByDisplayName(string displayName)
        {
            return profiles.FirstOrDefault(p => string.Equals(p.DisplayName, displayName, StringComparison.OrdinalIgnoreCase));
        }
    }
}
