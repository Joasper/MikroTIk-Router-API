using MikroClean.Domain.MikroTik;
using MikroClean.Domain.MikroTik.Operations;

namespace MikroClean.Application.MikroTik.Operations
{
    /// <summary>
    /// Operación para crear una interfaz Bridge en MikroTik
    /// </summary>
    public class CreateBridgeOperation : IMikroTikOperation<CreateBridgeRequest, BridgeResponse>
    {
        public string Command => "/interface/bridge/add";

        public Dictionary<string, string> BuildParameters(CreateBridgeRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["name"] = request.Name,
                ["admin-mac"] = request.AdminMac ? "yes" : "no",
                ["ageing-time"] = request.AgingTime.ToString(),
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.Comment))
            {
                parameters["comment"] = request.Comment;
            }

            return parameters;
        }

        public BridgeResponse ParseResponse(ITikSentence response)
        {
            return new BridgeResponse
            {
                Id = response.GetResponseField(".id"),
                Name = response.GetResponseField("name"),
                MacAddress = response.GetResponseField("mac-address") ?? string.Empty,
                Running = response.GetResponseField("running") == "true",
                Disabled = response.GetResponseField("disabled") == "true",
                Comment = response.GetResponseField("comment")
            };
        }
    }

    /// <summary>
    /// Operación para crear una interfaz VLAN
    /// </summary>
    public class CreateVlanOperation : IMikroTikOperation<CreateVlanRequest, VlanResponse>
    {
        public string Command => "/interface/vlan/add";

        public Dictionary<string, string> BuildParameters(CreateVlanRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["name"] = request.Name,
                ["vlan-id"] = request.VlanId.ToString(),
                ["interface"] = request.Interface,
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.Comment))
            {
                parameters["comment"] = request.Comment;
            }

            return parameters;
        }

        public VlanResponse ParseResponse(ITikSentence response)
        {
            return new VlanResponse
            {
                Id = response.GetResponseField(".id"),
                Name = response.GetResponseField("name"),
                VlanId = int.Parse(response.GetResponseField("vlan-id")),
                Interface = response.GetResponseField("interface"),
                Running = response.GetResponseField("running") == "true",
                Disabled = response.GetResponseField("disabled") == "true",
                Comment = response.GetResponseField("comment")
            };
        }
    }

    /// <summary>
    /// Query para obtener todas las interfaces
    /// </summary>
    public class GetAllInterfacesQuery : IMikroTikQuery<List<InterfaceResponse>>
    {
        public string Command => "/interface/print";

        public List<InterfaceResponse> ParseResponse(IEnumerable<ITikSentence> responses)
        {
            var interfaces = new List<InterfaceResponse>();

            foreach (var sentence in responses)
            {
                interfaces.Add(new InterfaceResponse
                {
                    Id = sentence.GetResponseField(".id"),
                    Name = sentence.GetResponseField("name"),
                    Type = sentence.GetResponseField("type"),
                    MacAddress = sentence.GetResponseField("mac-address") ?? string.Empty,
                    Running = sentence.GetResponseField("running") == "true",
                    Disabled = sentence.GetResponseField("disabled") == "true",
                    RxBytes = long.TryParse(sentence.GetResponseField("rx-byte"), out var rx) ? rx : 0,
                    TxBytes = long.TryParse(sentence.GetResponseField("tx-byte"), out var tx) ? tx : 0,
                    Comment = sentence.GetResponseField("comment")
                });
            }

            return interfaces;
        }
    }

    /// <summary>
    /// Operación para agregar una dirección IP
    /// </summary>
    public class CreateIpAddressOperation : IMikroTikOperation<CreateIpAddressRequest, IpAddressResponse>
    {
        public string Command => "/ip/address/add";

        public Dictionary<string, string> BuildParameters(CreateIpAddressRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["address"] = request.Address,
                ["interface"] = request.Interface,
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.Network))
            {
                parameters["network"] = request.Network;
            }

            if (!string.IsNullOrEmpty(request.Comment))
            {
                parameters["comment"] = request.Comment;
            }

            return parameters;
        }

        public IpAddressResponse ParseResponse(ITikSentence response)
        {
            return new IpAddressResponse
            {
                Id = response.GetResponseField(".id"),
                Address = response.GetResponseField("address"),
                Network = response.GetResponseField("network") ?? string.Empty,
                Interface = response.GetResponseField("interface"),
                Invalid = response.GetResponseField("invalid") == "true",
                Disabled = response.GetResponseField("disabled") == "true",
                Dynamic = response.GetResponseField("dynamic") == "true"
            };
        }
    }

    /// <summary>
    /// Query para obtener información del sistema
    /// </summary>
    public class GetSystemResourceQuery : IMikroTikQuery<SystemResourceResponse>
    {
        public string Command => "/system/resource/print";

        public SystemResourceResponse ParseResponse(IEnumerable<ITikSentence> responses)
        {
            var sentence = responses.FirstOrDefault();
            if (sentence == null)
                throw new InvalidOperationException("No se recibió respuesta del router");

            return new SystemResourceResponse
            {
                Version = sentence.GetResponseField("version"),
                BoardName = sentence.GetResponseField("board-name"),
                Architecture = sentence.GetResponseField("architecture-name"),
                TotalMemory = long.TryParse(sentence.GetResponseField("total-memory"), out var totalMem) ? totalMem : 0,
                FreeMemory = long.TryParse(sentence.GetResponseField("free-memory"), out var freeMem) ? freeMem : 0,
                CpuLoad = double.TryParse(sentence.GetResponseField("cpu-load"), out var cpuLoad) ? cpuLoad : 0,
                TotalHddSpace = long.TryParse(sentence.GetResponseField("total-hdd-space"), out var totalHdd) ? totalHdd : 0,
                FreeHddSpace = long.TryParse(sentence.GetResponseField("free-hdd-space"), out var freeHdd) ? freeHdd : 0,
                Uptime = ParseUptime(sentence.GetResponseField("uptime"))
            };
        }

        private TimeSpan ParseUptime(string uptime)
        {
            try
            {
                var totalSeconds = 0;
                var current = "";
                
                foreach (var c in uptime)
                {
                    if (char.IsDigit(c))
                    {
                        current += c;
                    }
                    else if (!string.IsNullOrEmpty(current))
                    {
                        var value = int.Parse(current);
                        totalSeconds += c switch
                        {
                            'w' => value * 7 * 24 * 3600,
                            'd' => value * 24 * 3600,
                            'h' => value * 3600,
                            'm' => value * 60,
                            's' => value,
                            _ => 0
                        };
                        current = "";
                    }
                }

                return TimeSpan.FromSeconds(totalSeconds);
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Operación para crear una regla de firewall
    /// </summary>
    public class CreateFirewallRuleOperation : IMikroTikOperation<CreateFirewallRuleRequest, FirewallRuleResponse>
    {
        public string Command => "/ip/firewall/filter/add";

        public Dictionary<string, string> BuildParameters(CreateFirewallRuleRequest request)
        {
            var parameters = new Dictionary<string, string>
            {
                ["chain"] = request.Chain,
                ["action"] = request.Action,
                ["disabled"] = request.Disabled ? "yes" : "no"
            };

            if (!string.IsNullOrEmpty(request.SrcAddress))
                parameters["src-address"] = request.SrcAddress;
            
            if (!string.IsNullOrEmpty(request.DstAddress))
                parameters["dst-address"] = request.DstAddress;
            
            if (!string.IsNullOrEmpty(request.Protocol))
                parameters["protocol"] = request.Protocol;
            
            if (!string.IsNullOrEmpty(request.DstPort))
                parameters["dst-port"] = request.DstPort;
            
            if (!string.IsNullOrEmpty(request.InInterface))
                parameters["in-interface"] = request.InInterface;
            
            if (!string.IsNullOrEmpty(request.OutInterface))
                parameters["out-interface"] = request.OutInterface;
            
            if (!string.IsNullOrEmpty(request.Comment))
                parameters["comment"] = request.Comment;

            return parameters;
        }

        public FirewallRuleResponse ParseResponse(ITikSentence response)
        {
            return new FirewallRuleResponse
            {
                Id = response.GetResponseField(".id"),
                Chain = response.GetResponseField("chain"),
                Action = response.GetResponseField("action"),
                SrcAddress = response.GetResponseField("src-address"),
                DstAddress = response.GetResponseField("dst-address"),
                Protocol = response.GetResponseField("protocol"),
                DstPort = response.GetResponseField("dst-port"),
                Bytes = long.TryParse(response.GetResponseField("bytes"), out var bytes) ? bytes : 0,
                Packets = long.TryParse(response.GetResponseField("packets"), out var packets) ? packets : 0,
                Disabled = response.GetResponseField("disabled") == "true",
                Invalid = response.GetResponseField("invalid") == "true",
                Comment = response.GetResponseField("comment")
            };
        }

        public class GetAllIpPoolsQuery : IMikroTikQuery<List<IpPoolResponse>>
        {
            public string Command => "/ip/pool/print";

            public List<IpPoolResponse> ParseResponse(IEnumerable<ITikSentence> responses)
            {
                var pools = new List<IpPoolResponse>();

                foreach (var sentence in responses)
                {
                    pools.Add(new IpPoolResponse
                    {
                        Id = sentence.GetResponseField(".id"),
                        Name = sentence.GetResponseField("name"),
                        Ranges = sentence.GetResponseField("ranges"),
                        NextPool = sentence.GetOptionalField("next-pool"),
                        Comment = sentence.GetOptionalField("comment")
                    });
                }

                return pools;
            }

            public class GetIpPoolByIdOperation : IMikroTikOperation<string, IpPoolResponse?>
            {
                public string Command => "/ip/pool/print";

                public Dictionary<string, string> BuildParameters(string id)
                {
                    return new Dictionary<string, string>
                    {
                        ["?.id"] = id
                    };
                }

                public IpPoolResponse? ParseResponse(ITikSentence response)
                {
                    if (response == null) return null;

                    return new IpPoolResponse
                    {
                        Id = response.GetResponseField(".id"),
                        Name = response.GetResponseField("name"),
                        Ranges = response.GetResponseField("ranges"),
                        NextPool = response.GetOptionalField("next-pool"),
                        Comment = response.GetOptionalField("comment")
                    };
                }
            }

            public class CreateIpPoolOperation : IMikroTikMutation<CreateIpPoolRequest, IpPoolResponse>
            {
                public string Command => "/ip/pool/add";
                public Dictionary<string, string> BuildParameters(CreateIpPoolRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["name"] = request.Name,
                        ["ranges"] = request.Ranges,
                    };

                    if (!string.IsNullOrEmpty(request.NextPool))
                    {
                        parameters["next-pool"] = request.NextPool;
                    }

                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                
                // ParseResponse recibe el ID directamente ("*9")
                public IpPoolResponse ParseResponse(string? rawResponse)
                {
                    return new IpPoolResponse { Id = rawResponse ?? string.Empty };
                }
            }

            public class UpdateIpPoolOperation : IMikroTikMutation<UpdateIpPoolRequest, IpPoolResponse>
            {
                public string Command => "/ip/pool/set";

                public Dictionary<string, string> BuildParameters(UpdateIpPoolRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "numbers", request.Id.ToLower().Trim() }
                    };

                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        parameters["name"] = request.Name;
                    }


                    if (!string.IsNullOrEmpty(request.Ranges))
                    {
                        parameters["ranges"] = request.Ranges;
                    }

                    if (!string.IsNullOrWhiteSpace(request.NextPool))
                    {
                        parameters["next-pool"] = request.NextPool;
                    }

                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }


                    return parameters;

                }

                // set devuelve vacio, retornamos un objeto vacio, el servicio hara GetById
                public IpPoolResponse ParseResponse(string? rawResponse)
                {
                    return new IpPoolResponse();
                }

            }

            public class DeleteIpPoolOperation : IMikroTikMutation<DeleteIpPoolRequest, IpPoolResponse>
            {
                public string Command => "/ip/pool/remove";

                public Dictionary<string, string> BuildParameters(DeleteIpPoolRequest request)
                {
                    return new Dictionary<string, string>
                    {
                        [".id"] = request.Id
                    };
                }

                // remove devuelve vacio
                public IpPoolResponse ParseResponse(string? rawResponse)
                {
                    return new IpPoolResponse();
                }
            }


            public class CreatePPPoEProfileOperation : IMikroTikMutation<CreatePPPoEProfile, PPPoEProfileResponse>
            {
                public string Command => "/ppp/profile/add";
                public Dictionary<string, string> BuildParameters(CreatePPPoEProfile request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["name"] = request.Name,
                        ["local-address"] = request.LocalAddress,
                        ["remote-address"] = request.RemoteAddress,
                        ["dns-server"] = request.DnsServers,
                        ["rate-limit"] = request.RateLimit,
                        ["only-one"] = request.OnlyOne,
                        //["disabled"] = request.Disabled ? "yes" : "no"
                    };
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoEProfileResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEProfileResponse { Id = rawResponse ?? string.Empty };
                }
            }

            public class GetPppProfileByIdOperation : IMikroTikOperation<string, PPPoEProfileResponse?>
            {
                public string Command => "/ppp/profile/print";
                public Dictionary<string, string> BuildParameters(string id)
                {
                    return new Dictionary<string, string>
                    {
                        ["?.id"] = id
                    };
                }
                public PPPoEProfileResponse? ParseResponse(ITikSentence response)
                {
                    if (response == null) return null;
                    return new PPPoEProfileResponse
                    {
                        Id = response.GetResponseField(".id"),
                        Name = response.GetResponseField("name"),
                        LocalAddress = response.GetResponseField("local-address"),
                        RemoteAddress = response.GetResponseField("remote-address"),
                        DnsServers = response.GetResponseField("dns-server"),
                        RateLimit = response.GetResponseField("rate-limit"),
                        OnlyOne = response.GetResponseField("only-one"),
                        Comment = response.GetOptionalField("comment")
                    };
                }
            }

            public class GetAllPppProfilesQuery : IMikroTikQuery<List<PPPoEProfileResponse>>
            {
                public string Command => "/ppp/profile/print";
                public List<PPPoEProfileResponse> ParseResponse(IEnumerable<ITikSentence> responses)
                {
                    var profiles = new List<PPPoEProfileResponse>();
                    foreach (var sentence in responses)
                    {
                        profiles.Add(new PPPoEProfileResponse
                        {
                            Id = sentence.GetResponseField(".id"),
                            Name = sentence.GetResponseField("name"),
                            LocalAddress = sentence.GetOptionalField("local-address"),
                            RemoteAddress = sentence.GetOptionalField("remote-address"),
                            DnsServers = sentence.GetOptionalField("dns-server"),
                            RateLimit = sentence.GetOptionalField("rate-limit"),
                            OnlyOne = sentence.GetOptionalField("only-one"),
                            Comment = sentence.GetOptionalField("comment")
                        });
                    }
                    return profiles;
                }
            }

            public class DeletePppProfileOperation : IMikroTikMutation<DeletePPPoEProfile, PPPoEProfileResponse>
            {
                public string Command => "/ppp/profile/remove";
                public Dictionary<string, string> BuildParameters(DeletePPPoEProfile parameters)
                {
                    return new Dictionary<string, string>
                    {
                        [".id"] = parameters.Id.Trim().ToLower()
                    };
                }
                public PPPoEProfileResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEProfileResponse();
                }
            }

            public class UpdatePppProfileOperation : IMikroTikMutation<UpdatePPPoEProfile, PPPoEProfileResponse>
            {
                public string Command => "/ppp/profile/set";
                public Dictionary<string, string> BuildParameters(UpdatePPPoEProfile request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "numbers", request.Id.ToLower().Trim() }
                    };
                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        parameters["name"] = request.Name;
                    }
                    if (!string.IsNullOrEmpty(request.LocalAddress))
                    {
                        parameters["local-address"] = request.LocalAddress;
                    }
                    if (!string.IsNullOrEmpty(request.RemoteAddress))
                    {
                        parameters["remote-address"] = request.RemoteAddress;
                    }
                    if (!string.IsNullOrEmpty(request.DnsServers))
                    {
                        parameters["dns-server"] = request.DnsServers;
                    }
                    if (!string.IsNullOrEmpty(request.RateLimit))
                    {
                        parameters["rate-limit"] = request.RateLimit;
                    }
                    if (!string.IsNullOrEmpty(request.OnlyOne))
                    {
                        parameters["only-one"] = request.OnlyOne;
                    }
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoEProfileResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEProfileResponse();
                }
            }


            public class CreatePPPoESecretOperation : IMikroTikMutation<CreatePPPoESecretRequest, PPPoESecretResponse>
            {
                public string Command => "/ppp/secret/add";
                public Dictionary<string, string> BuildParameters(CreatePPPoESecretRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["name"] = request.Name,
                        ["password"] = request.Password,
                        ["profile"] = request.Profile,
                        ["service"] = request.Service,
                    };
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoESecretResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoESecretResponse { Id = rawResponse ?? string.Empty };
                }
            }

            public class GetPppSecretByIdOperation : IMikroTikOperation<string, PPPoESecretResponse?>
            {
                public string Command => "/ppp/secret/print";
                public Dictionary<string, string> BuildParameters(string id)
                {
                    return new Dictionary<string, string>
                    {
                        ["?.id"] = id
                    };
                }
                public PPPoESecretResponse? ParseResponse(ITikSentence response)
                {
                    if (response == null) return null;
                    return new PPPoESecretResponse
                    {
                        Id = response.GetResponseField(".id"),
                        Name = response.GetResponseField("name"),
                        Profile = response.GetResponseField("profile"),
                        Service = response.GetResponseField("service"),
                        Comment = response.GetOptionalField("comment")
                    };
                }
            }

            public class GetAllPppSecretsQuery : IMikroTikQuery<List<PPPoESecretResponse>>
            {
                public string Command => "/ppp/secret/print";
                public List<PPPoESecretResponse> ParseResponse(IEnumerable<ITikSentence> responses)
                {
                    var secrets = new List<PPPoESecretResponse>();
                    foreach (var sentence in responses)
                    {
                        secrets.Add(new PPPoESecretResponse
                        {
                            Id = sentence.GetResponseField(".id"),
                            Name = sentence.GetResponseField("name"),
                            Profile = sentence.GetResponseField("profile"),
                            Service = sentence.GetResponseField("service"),
                            Comment = sentence.GetOptionalField("comment")
                        });
                    }
                    return secrets;
                }
            }

            public class DeletePppSecretOperation : IMikroTikMutation<DeletePPPoESecretRequest, PPPoESecretResponse>
            {
                public string Command => "/ppp/secret/remove";
                public Dictionary<string, string> BuildParameters(DeletePPPoESecretRequest parameters)
                {
                    return new Dictionary<string, string>
                    {
                        [".id"] = parameters.Id.Trim().ToLower()
                    };
                }
                public PPPoESecretResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoESecretResponse();
                }
            }

            public class UpdatePppSecretOperation : IMikroTikMutation<UpdatePPPoESecretRequest, PPPoESecretResponse>
            {
                public string Command => "/ppp/secret/set";
                public Dictionary<string, string> BuildParameters(UpdatePPPoESecretRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "numbers", request.Id.ToLower().Trim() }
                    };
                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        parameters["name"] = request.Name;
                    }
                    if (!string.IsNullOrEmpty(request.Password))
                    {
                        parameters["password"] = request.Password;
                    }
                    if (!string.IsNullOrEmpty(request.Profile))
                    {
                        parameters["profile"] = request.Profile;
                    }
                    if (!string.IsNullOrEmpty(request.Service))
                    {
                        parameters["service"] = request.Service;
                    }
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoESecretResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoESecretResponse();
                }
            }


            public class CreatePPPoEServerOperation : IMikroTikMutation<CreatePPPoEServerRequest, PPPoEServerResponse>
            {
                public string Command => "/interface/pppoe-server/server/add";
                public Dictionary<string, string> BuildParameters(CreatePPPoEServerRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        ["service-name"] = request.Name,
                        ["interface"] = request.Interface,
                        ["default-profile"] = request.Profile,
                        ["max-mtu"] = request.MaxMTU,
                        ["max-mru"] = request.MaxMRU,
                        ["keepalive-timeout"] = request.KeepAliveTimeOut,
                        ["one-session-per-host"] = request.OneSesionPerHost,
                    };
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoEServerResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEServerResponse { Id = rawResponse ?? string.Empty };
                }
            }

            public class GetPppServerByIdOperation : IMikroTikOperation<string, PPPoEServerResponse?>
            {
                public string Command => "/interface/pppoe-server/server/print";
                public Dictionary<string, string> BuildParameters(string id)
                {
                    return new Dictionary<string, string>
                    {
                        ["?.id"] = id
                    };
                }
                public PPPoEServerResponse? ParseResponse(ITikSentence response)
                {
                    if (response == null) return null;
                    return new PPPoEServerResponse
                    {
                        Id = response.GetResponseField(".id"),
                        Name = response.GetResponseField("service-name"),
                        Interface = response.GetResponseField("interface"),
                        Profile = response.GetResponseField("default-profile"),
                        MaxMTU = response.GetResponseField("max-mtu"),
                        MaxMRU = response.GetResponseField("max-mru"),
                        KeepAliveTimeOut = response.GetResponseField("keepalive-timeout"),
                        OneSesionPerHost = response.GetResponseField("one-session-per-host"),
                        Comment = response.GetOptionalField("comment")
                    };
                }
            }

            public class GetAllPppServersQuery : IMikroTikQuery<List<PPPoEServerResponse>>
            {
                public string Command => "/interface/pppoe-server/server/print";
                public List<PPPoEServerResponse> ParseResponse(IEnumerable<ITikSentence> responses)
                {
                    var servers = new List<PPPoEServerResponse>();
                    foreach (var sentence in responses)
                    {
                        servers.Add(new PPPoEServerResponse
                        {
                            Id = sentence.GetResponseField(".id"),
                            Name = sentence.GetResponseField("service-name"),
                            Interface = sentence.GetResponseField("interface"),
                            Profile = sentence.GetResponseField("default-profile"),
                            MaxMTU = sentence.GetResponseField("max-mtu"),
                            MaxMRU = sentence.GetResponseField("max-mru"),
                            KeepAliveTimeOut = sentence.GetResponseField("keepalive-timeout"),
                            OneSesionPerHost = sentence.GetResponseField("one-session-per-host"),
                            Comment = sentence.GetOptionalField("comment")
                        });
                    }
                    return servers;
                }
            }

            public class DeletePppServerOperation : IMikroTikMutation<DeletePPPoEServerRequest, PPPoEServerResponse>
            {
                public string Command => "/interface/pppoe-server/server/remove";
                public Dictionary<string, string> BuildParameters(DeletePPPoEServerRequest parameters)
                {
                    return new Dictionary<string, string>
                    {
                        [".id"] = parameters.Id.Trim().ToLower()
                    };
                }
                public PPPoEServerResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEServerResponse();
                }
            }

            public class UpdatePppServerOperation : IMikroTikMutation<UpdatePPPoEServerRequest, PPPoEServerResponse>
            {
                public string Command => "/interface/pppoe-server/server/set";
                public Dictionary<string, string> BuildParameters(UpdatePPPoEServerRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "numbers", request.Id.ToLower().Trim() }
                    };
                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        parameters["service-name"] = request.Name;
                    }
                    if (!string.IsNullOrEmpty(request.Interface))
                    {
                        parameters["interface"] = request.Interface;
                    }
                    if (!string.IsNullOrEmpty(request.Profile))
                    {
                        parameters["default-profile"] = request.Profile;
                    }
                    if (!string.IsNullOrEmpty(request.MaxMTU))
                    {
                        parameters["max-mtu"] = request.MaxMTU;
                    }
                    if (!string.IsNullOrEmpty(request.MaxMRU))
                    {
                        parameters["max-mru"] = request.MaxMRU;
                    }
                    if (!string.IsNullOrEmpty(request.KeepAliveTimeOut))
                    {
                        parameters["keepalive-timeout"] = request.KeepAliveTimeOut;
                    }
                    if (!string.IsNullOrEmpty(request.OneSesionPerHost))
                    {
                        parameters["one-session-per-host"] = request.OneSesionPerHost;
                    }
                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Comment;
                    }
                    return parameters;
                }
                public PPPoEServerResponse ParseResponse(string? rawResponse)
                {
                    return new PPPoEServerResponse();
                }
            }


            //private static string GetRequiredField(ITikSentence sentence, string fieldName)
            //{
            //    return sentence.GetResponseField(fieldName);
            //}

            //private static string GetOptionalField(ITikSentence sentence, string fieldName, string defaultValue = "")
            //{
            //    try
            //    {
            //        return sentence.GetResponseField(fieldName) ?? defaultValue;
            //    }
            //    catch
            //    {
            //        return defaultValue;
            //    }
            //}
        }

     
    }
}
