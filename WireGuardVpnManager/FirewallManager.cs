using System;
namespace WireGuardVpnManager
{
    public class FirewallManager
    {
        private const int NET_FW_RULE_DIR_OUT = 2;
        private const int NET_FW_ACTION_BLOCK = 0;
        private const int NET_FW_ACTION_ALLOW = 1;
        private const int NET_FW_IP_PROTOCOL_UDP = 17;
        private const int NET_FW_PROFILE2_ALL = 0x7;
        public void AddBlockAllRule()
        {
            try
            {
                var fwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (fwPolicy2Type == null) throw new InvalidOperationException("COM type HNetCfg.FwPolicy2 not available.");
                dynamic fwPolicy2 = Activator.CreateInstance(fwPolicy2Type);
                dynamic rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                if (ruleType == null) throw new InvalidOperationException("COM type HNetCfg.FWRule not available.");
                dynamic rule = Activator.CreateInstance(ruleType);
                rule.Name = "BlockAllOutbound-VGKM";
                rule.Direction = NET_FW_RULE_DIR_OUT;
                rule.Action = NET_FW_ACTION_BLOCK;
                rule.Enabled = true;
                rule.Profiles = NET_FW_PROFILE2_ALL;
                rule.InterfaceTypes = "All";
                rules.Add(rule);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add block-all firewall rule: {ex.Message}", ex);
            }
        }
        public void RemoveRule(string ruleName)
        {
            try
            {
                var fwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (fwPolicy2Type == null) return;
                dynamic fwPolicy2 = Activator.CreateInstance(fwPolicy2Type);
                dynamic rules = fwPolicy2.Rules;
                try { rules.Remove(ruleName); } catch { }
            }
            catch
            {
                // ignore errors during remove
            }
        }
        public void AddAllowEndpointRule(string ruleName, string ipOrHost, int port)
        {
            try
            {
                var fwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (fwPolicy2Type == null) throw new InvalidOperationException("COM type HNetCfg.FwPolicy2 not available.");
                dynamic fwPolicy2 = Activator.CreateInstance(fwPolicy2Type);
                dynamic rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                if (ruleType == null) throw new InvalidOperationException("COM type HNetCfg.FWRule not available.");
                dynamic rule = Activator.CreateInstance(ruleType);
                rule.Name = ruleName;
                rule.Direction = NET_FW_RULE_DIR_OUT;
                rule.Action = NET_FW_ACTION_ALLOW;
                rule.Enabled = true;
                rule.Protocol = NET_FW_IP_PROTOCOL_UDP;
                rule.RemoteAddresses = ipOrHost;
                rule.RemotePorts = port.ToString();
                rule.Profiles = NET_FW_PROFILE2_ALL;
                rule.InterfaceTypes = "All";
                rules.Add(rule);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add allow-endpoint firewall rule: {ex.Message}", ex);
            }
        }
        public void AddAllowExeRule(string ruleName, string exePath)
        {
            try
            {
                var fwPolicy2Type = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
                if (fwPolicy2Type == null) throw new InvalidOperationException("COM type HNetCfg.FwPolicy2 not available.");
                dynamic fwPolicy2 = Activator.CreateInstance(fwPolicy2Type);
                dynamic rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
                if (ruleType == null) throw new InvalidOperationException("COM type HNetCfg.FWRule not available.");
                dynamic rule = Activator.CreateInstance(ruleType);
                rule.Name = ruleName;
                rule.Direction = NET_FW_RULE_DIR_OUT;
                rule.Action = NET_FW_ACTION_ALLOW;
                rule.Enabled = true;
                rule.ApplicationName = exePath;
                rule.Profiles = NET_FW_PROFILE2_ALL;
                rule.InterfaceTypes = "All";
                rules.Add(rule);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to add allow-exe firewall rule: {ex.Message}", ex);
            }
        }
    }
}
