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
                var fwPolicy2 = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                var rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
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
                var fwPolicy2 = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                var rules = fwPolicy2.Rules;
                rules.Remove(ruleName);
            }
            catch
            {
            }
        }

        public void AddAllowEndpointRule(string ruleName, string ipOrHost, int port)
        {
            try
            {
                var fwPolicy2 = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                var rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
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
                var fwPolicy2 = Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
                var rules = fwPolicy2.Rules;
                var ruleType = Type.GetTypeFromProgID("HNetCfg.FWRule");
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
