# 1970-01-02 00:33:57 by RouterOS 7.16.2
# software id = 2SVX-FWND
#
# model = RB941-2nD
# serial number = HJD0AENGF33
/interface bridge
add admin-mac=F4:1E:57:F6:0E:F0 auto-mac=no comment=defconf name=bridgeLocal
/interface wireless security-profiles
set [ find default=yes ] supplicant-identity=MikroTik
add authentication-types=wpa2-psk mode=dynamic-keys name=clave-casa \
    supplicant-identity=""
/interface wireless
set [ find default-name=wlan1 ] disabled=no mode=ap-bridge security-profile=\
    clave-casa ssid=MikroTik
/ip pool
add name=dhcp_pool0 ranges=192.168.88.2-192.168.88.254
/ip dhcp-server
add address-pool=dhcp_pool0 interface=bridgeLocal name=dhcp1
/interface bridge port
add bridge=bridgeLocal comment=defconf interface=ether1
add bridge=bridgeLocal comment=defconf interface=ether2
add bridge=bridgeLocal comment=defconf interface=ether3
add bridge=bridgeLocal comment=defconf interface=ether4
add bridge=bridgeLocal interface=wlan1
/interface wireless cap
set bridge=bridgeLocal discovery-interfaces=bridgeLocal interfaces=wlan1
/ip address
add address=192.168.88.1/24 interface=bridgeLocal network=192.168.88.0
/ip dhcp-client
add comment=defconf interface=bridgeLocal
/ip dhcp-server network
add address=192.168.88.0/24 dns-server=8.8.8.8 gateway=192.168.88.1
/ip service
set www-ssl disabled=no
/system note
set show-at-login=no
