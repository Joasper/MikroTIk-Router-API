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

            public class GetIpPoolByIdOperation : IMikroTikQuery<IpPoolResponse?>
            {
                private readonly string _id;

                public GetIpPoolByIdOperation(string id)
                {
                    _id = id;
                }

                public string Command => $"/ip/pool/print ?.id={_id}";

                public IpPoolResponse? ParseResponse(IEnumerable<ITikSentence> responses)
                {
                    var sentence = responses.FirstOrDefault();
                    if (sentence == null) return null;

                    return new IpPoolResponse
                    {
                        Id = sentence.GetResponseField(".id"),
                        Name = sentence.GetResponseField("name"),
                        Ranges = sentence.GetResponseField("ranges"),
                        NextPool = sentence.GetOptionalField("next-pool"),
                        Comment = sentence.GetOptionalField("comment")
                    };
                }
            }

            public class CreateIpPoolOperation : IMikroTikOperation<CreateIpPoolRequest, string>
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
                public string ParseResponse(ITikSentence response)
                {
                    return response.ToString();
                }
            }

            public class UpdateIpPoolOperation : IMikroTikOperation<UpdateIpPoolRequest, IpPoolResponse>
            {
                public string Command => "/ip/pool/set";

                public Dictionary<string, string> BuildParameters(UpdateIpPoolRequest request)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        [".id"] = request.Id
                    };

                    if (!string.IsNullOrEmpty(request.Name))
                    {
                        parameters["name"] = request.Name;
                    }


                    if (!string.IsNullOrEmpty(request.Ranges))
                    {
                        parameters["ranges"] = request.Ranges;
                    }

                    if (request.NextPool != null)
                    {
                        parameters["next-pool"] = request.Ranges;
                    }

                    if (!string.IsNullOrEmpty(request.Comment))
                    {
                        parameters["comment"] = request.Ranges;
                    }


                    return parameters;

                }

                public IpPoolResponse ParseResponse(ITikSentence response)
                {
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

            public class DeleteIpPoolOperation : IMikroTikOperation<DeleteIpPoolRequest, IpPoolResponse>
            {
                public string Command => "/ip/pool/remove";

                public Dictionary<string, string> BuildParameters(DeleteIpPoolRequest request)
                {
                    return new Dictionary<string, string>
                    {
                        [".id"] = request.Id
                    };
                }

                public IpPoolResponse ParseResponse(ITikSentence response)
                {
                    return new IpPoolResponse
                    {
                        Id = response.GetResponseField(".id") ?? string.Empty
                    };
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
