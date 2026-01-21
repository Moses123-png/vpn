#!/bin/bash
# setup-wg-ubuntu.sh
# Usage: sudo bash setup-wg-ubuntu.sh <client-name>

set -e

if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root (sudo)." 
   exit 1
fi

CLIENT_NAME="${1:-client1}"
SERVER_PORT=51820
WG_INTERFACE="wg0"
SERVER_PRIV_KEY_FILE="/etc/wireguard/server_private.key"
SERVER_PUB_KEY_FILE="/etc/wireguard/server_public.key"
CLIENT_PRIV_KEY_FILE="/etc/wireguard/${CLIENT_NAME}_private.key"
CLIENT_PUB_KEY_FILE="/etc/wireguard/${CLIENT_NAME}_public.key"
CLIENT_CONF_OUT="/root/${CLIENT_NAME}.conf"

apt update
apt install -y wireguard iproute2

# Generate keys
umask 077
wg genkey | tee ${SERVER_PRIV_KEY_FILE} | wg pubkey > ${SERVER_PUB_KEY_FILE}
wg genkey | tee ${CLIENT_PRIV_KEY_FILE} | wg pubkey > ${CLIENT_PUB_KEY_FILE}

SERVER_PRIV_KEY=$(cat ${SERVER_PRIV_KEY_FILE})
SERVER_PUB_KEY=$(cat ${SERVER_PUB_KEY_FILE})
CLIENT_PRIV_KEY=$(cat ${CLIENT_PRIV_KEY_FILE})
CLIENT_PUB_KEY=$(cat ${CLIENT_PUB_KEY_FILE})

SERVER_PUB_IP=$(curl -s https://api.ipify.org)
if [ -z "$SERVER_PUB_IP" ]; then
  echo "Could not determine public IP. Set SERVER_PUB_IP manually in the client conf."
  SERVER_PUB_IP="YOUR.SERVER.IP"
fi

cat > /etc/wireguard/${WG_INTERFACE}.conf <<EOF
[Interface]
Address = 10.200.200.1/24
ListenPort = ${SERVER_PORT}
PrivateKey = ${SERVER_PRIV_KEY}
SaveConfig = true
EOF

cat >> /etc/wireguard/${WG_INTERFACE}.conf <<EOF

[Peer]
# ${CLIENT_NAME}
PublicKey = ${CLIENT_PUB_KEY}
AllowedIPs = 10.200.200.2/32
EOF

chmod 600 /etc/wireguard/${WG_INTERFACE}.conf
systemctl enable wg-quick@${WG_INTERFACE}
systemctl start wg-quick@${WG_INTERFACE}

sysctl -w net.ipv4.ip_forward=1
iptables -t nat -A POSTROUTING -s 10.200.200.0/24 -o eth0 -j MASQUERADE

apt install -y iptables-persistent

cat > ${CLIENT_CONF_OUT} <<EOF
[Interface]
PrivateKey = ${CLIENT_PRIV_KEY}
Address = 10.200.200.2/32
DNS = 1.1.1.1

[Peer]
PublicKey = ${SERVER_PUB_KEY}
Endpoint = ${SERVER_PUB_IP}:${SERVER_PORT}
AllowedIPs = 0.0.0.0/0, ::/0
PersistentKeepalive = 25
EOF

chmod 600 ${CLIENT_CONF_OUT}
echo "Client config written to ${CLIENT_CONF_OUT}. Transfer it securely to your Windows machine and import into the manager."
echo "Server public IP: ${SERVER_PUB_IP}"
