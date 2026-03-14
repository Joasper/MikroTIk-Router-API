namespace MikroClean.Domain.MikroTik.Operations
{
    // ============= INTERFACES =============
    
    /// <summary>
    /// Request para crear una interfaz bridge
    /// </summary>
    public class CreateBridgeRequest
    {
        public string Name { get; set; } = string.Empty;
        public bool AdminMac { get; set; } = false;
        public int AgingTime { get; set; } = 300;
        public string? Comment { get; set; }
        public bool Disabled { get; set; } = false;
    }

    public class BridgeResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public bool Running { get; set; }
        public bool Disabled { get; set; }
        public string? Comment { get; set; }
    }

    public class CreateVlanRequest
    {
        public string Name { get; set; } = string.Empty;
        public int VlanId { get; set; }
        public string Interface { get; set; } = string.Empty;
        public bool Disabled { get; set; } = false;
        public string? Comment { get; set; }
    }

    public class VlanResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int VlanId { get; set; }
        public string Interface { get; set; } = string.Empty;
        public bool Running { get; set; }
        public bool Disabled { get; set; }
        public string? Comment { get; set; }
    }

    public class InterfaceResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public bool Running { get; set; }
        public bool Disabled { get; set; }
        public long RxBytes { get; set; }
        public long TxBytes { get; set; }
        public string? Comment { get; set; }
    }

    // ============= IP ADDRESS =============
    
    public class CreateIpAddressRequest
    {
        public string Address { get; set; } = string.Empty; // Formato: 192.168.1.1/24
        public string Interface { get; set; } = string.Empty;
        public string? Network { get; set; }
        public bool Disabled { get; set; } = false;
        public string? Comment { get; set; }
    }

    public class IpAddressResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Network { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public bool Invalid { get; set; }
        public bool Disabled { get; set; }
        public bool Dynamic { get; set; }
    }

    // ============= DHCP SERVER =============
    
    public class CreateDhcpServerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string AddressPool { get; set; } = string.Empty;
        public int LeaseTime { get; set; } = 600; // segundos
        public bool Disabled { get; set; } = false;
    }

    public class DhcpServerResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string AddressPool { get; set; } = string.Empty;
        public int LeaseTime { get; set; }
        public bool Disabled { get; set; }
        public bool Invalid { get; set; }
    }

    // ============= FIREWALL =============
    
    public class CreateFirewallRuleRequest
    {
        public string Chain { get; set; } = string.Empty; // forward, input, output
        public string Action { get; set; } = "accept"; // accept, drop, reject
        public string? SrcAddress { get; set; }
        public string? DstAddress { get; set; }
        public string? Protocol { get; set; }
        public string? DstPort { get; set; }
        public string? InInterface { get; set; }
        public string? OutInterface { get; set; }
        public string? Comment { get; set; }
        public bool Disabled { get; set; } = false;
    }

    public class FirewallRuleResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Chain { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? SrcAddress { get; set; }
        public string? DstAddress { get; set; }
        public string? Protocol { get; set; }
        public string? DstPort { get; set; }
        public long Bytes { get; set; }
        public long Packets { get; set; }
        public bool Disabled { get; set; }
        public bool Invalid { get; set; }
        public string? Comment { get; set; }
    }

    // ============= SYSTEM INFO =============
    
    public class SystemResourceResponse
    {
        public string Version { get; set; } = string.Empty;
        public string BoardName { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public long TotalMemory { get; set; }
        public long FreeMemory { get; set; }
        public double CpuLoad { get; set; }
        public long TotalHddSpace { get; set; }
        public long FreeHddSpace { get; set; }
        public TimeSpan Uptime { get; set; }
    }

    public class SystemIdentityResponse
    {
        public string Name { get; set; } = string.Empty;
    }

    // ============= WIRELESS =============
    
    public class CreateWirelessNetworkRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Ssid { get; set; } = string.Empty;
        public string Mode { get; set; } = "ap-bridge"; // ap-bridge, station
        public string Band { get; set; } = "2ghz-b/g/n";
        public int? Channel { get; set; }
        public string? Security { get; set; } // wpa2, wpa3
        public string? Password { get; set; }
        public bool Disabled { get; set; } = false;
        public string? Comment { get; set; }
    }

    public class WirelessNetworkResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Ssid { get; set; } = string.Empty;
        public string Mode { get; set; } = string.Empty;
        public string Band { get; set; } = string.Empty;
        public int? Channel { get; set; }
        public bool Running { get; set; }
        public bool Disabled { get; set; }
        public int ConnectedClients { get; set; }
    }

    // ============= Pools =============
    
    public class CreateIpPoolRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Ranges { get; set; } = string.Empty;
        public string? NextPool { get; set; }
        public string? Comment { get; set; }
    }

    public class UpdateIpPoolRequest
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Ranges { get; set; }
        public string? NextPool { get; set; }
        public string? Comment { get; set; }
    }


    public class GetIpPoolRequest
    {
        public string Id { get; set; }
    }
    public class DeleteIpPoolRequest
    {
        public string Id { get; set; }
    }

    public class IpPoolResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Ranges { get; set; } = string.Empty;
        public string NextPool { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;


    }

    // ============= PPP =============

    public class CreatePPPoEProfile
    {
        public string Name { get; set; } = string.Empty;
        public string LocalAddress { get; set; } = string.Empty;
        public string RemoteAddress { get; set; } = string.Empty;
        public string DnsServers { get; set; } = string.Empty;
        public string RateLimit { get; set; } = string.Empty;
        public string OnlyOne { get; set; } = "default";
        public string Comment { get; set; } = string.Empty;

    }

    public class UpdatePPPoEProfile
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? LocalAddress { get; set; }
        public string? RemoteAddress { get; set; }
        public string? DnsServers { get; set; }
        public string? RateLimit { get; set; }
        public string? OnlyOne { get; set; }
        public string? Comment { get; set; }
    }

    public class DeletePPPoEProfile
    {
        public string Id { get; set; }
    }


    public class PPPoEProfileResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string LocalAddress { get; set; } = string.Empty;
        public string RemoteAddress { get; set; } = string.Empty;
        public string DnsServers { get; set; } = string.Empty;
        public string RateLimit { get; set; } = string.Empty;
        public string OnlyOne { get; set; } = "default";
        public string Comment { get; set; } = string.Empty;

    }


    public class CreatePPPoESecretRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Service { get; set; } = "pppoe";
        public string Profile { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
    }


    public class PPPoESecretResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Service { get; set; } = string.Empty;
        public string Profile { get; set; } = string.Empty;
        public bool Disabled { get; set; }
        public string Comment { get; set; } = string.Empty;
    }

    public class UpdatePPPoESecretRequest
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Service { get; set; }
        public string? Profile { get; set; }
        public string? Comment { get; set; }
    }

    public class DeletePPPoESecretRequest
    {
        public string Id { get; set; }
    }


    public class CreatePPPoEServerRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string Profile { get; set; } = "default";
        public string MaxMTU { get; set; } = "1480";
        public string MaxMRU { get; set; } = "1480";
        public string KeepAliveTimeOut { get; set; } = "10";
        public string OneSesionPerHost { get; set; } = "yes";
        public string Comment { get; set; } = string.Empty;
    }

    public class PPPoEServerResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Interface { get; set; } = string.Empty;
        public string Profile { get; set; } = "default";
        public string MaxMTU { get; set; } = "1480";
        public string MaxMRU { get; set; } = "1480";
        public string KeepAliveTimeOut { get; set; } = "10";
        public string OneSesionPerHost { get; set; } = "yes";
        public bool Disabled { get; set; }
        public string Comment { get; set; } = string.Empty;

    }

    public class UpdatePPPoEServerRequest
    {
        public string Id { get; set; }
        public string? Name { get; set; }
        public string? Interface { get; set; }
        public string? Profile { get; set; }
        public string? MaxMTU { get; set; }
        public string? MaxMRU { get; set; }
        public string? KeepAliveTimeOut { get; set; }
        public string? OneSesionPerHost { get; set; }
        public string? Comment { get; set; }
    }

    public class DeletePPPoEServerRequest
    {
        public string Id { get; set; }
    }

}