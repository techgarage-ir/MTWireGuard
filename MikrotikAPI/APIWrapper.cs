using MikrotikAPI.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MikrotikAPI
{
    public class APIWrapper
    {
        private string MT_IP { get; set; }
        private string MT_USER { get; set; }
        private string MT_PASS { get; set; }

        public APIWrapper(string IP, string User, string Password)
        {
            MT_IP = IP;
            MT_USER = User;
            MT_PASS = Password;
        }

        public async Task<List<Log>> GetLogsAsync()
        {
            string json = await SendGetRequestAsync(Endpoints.Log);
            return json.ToModel<List<Log>>();
        }

        public async Task<List<WGServer>> GetServersAsync()
        {
            string json = await SendGetRequestAsync(Endpoints.Wireguard);
            return json.ToModel<List<WGServer>>();
        }

        public async Task<WGServer> GetServer(string Name)
        {
            var json = await SendGetRequestAsync(Endpoints.Wireguard + "?name=" + Name);
            return json.ToModel<WGServer[]>().FirstOrDefault();
        }

        public async Task<WGServer> GetServerById(string id)
        {
            var json = await SendGetRequestAsync(Endpoints.Wireguard + "/" + id);
            return json.ToModel<WGServer>();
        }

        public async Task<List<ServerTraffic>> GetServersTraffic()
        {
            var json = await SendRequestBase(RequestMethod.GET, Endpoints.Interface, "{\"stats\", {\".proplist\":\"name, type, rx-byte, tx-byte\"}}");
            return json.ToModel<List<ServerTraffic>>();
        }

        public async Task<string> GetIPAddresses()
        {
            var json = await SendGetRequestAsync(Endpoints.IPAddress);
            return json;
        }

        public async Task<CreationStatus> CreateIPAddress(IPAddressCreateModel ipAddress)
        {
            return await CreateItem<IPAddress>(Endpoints.IPAddress, ipAddress);
        }

        public async Task<CreationStatus> UpdateIPAddress(IPAddressUpdateModel ipAddress)
        {
            var itemJson = JObject.FromObject(ipAddress, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.IPAddress, itemJson, ipAddress.Id);
        }

        public async Task<CreationStatus> DeleteIP(string id)
        {
            return await DeleteItem(Endpoints.IPAddress, id);
        }

        public async Task<List<IPAddress>> GetServerIPAddress(string Interface)
        {
            var json = await SendGetRequestAsync(Endpoints.IPAddress + "?interface=" + Interface);
            return json.ToModel<List<IPAddress>>();
        }

        public async Task<List<WGPeer>> GetUsersAsync()
        {
            string json = await SendGetRequestAsync(Endpoints.WireguardPeers);
            return json.ToModel<List<WGPeer>>();
        }

        public async Task<WGPeer> GetUser(string id)
        {
            var users = await GetUsersAsync();
            return users.Find(u => u.Id == id);
        }

        public async Task<WGPeer> GetUserByPublicKey(string key)
        {
            var json = await SendGetRequestAsync(Endpoints.WireguardPeers + "?public-key=" + key);
            return json.ToModel<WGPeer[]>().FirstOrDefault();
        }

        public async Task<WGPeerLastHandshake> GetUserHandshake(string id)
        {
            var json = await SendRequestBase(RequestMethod.GET, Endpoints.WireguardPeers + $"/{id}?.proplist=last-handshake");
            return json.ToModel<WGPeerLastHandshake>();
        }

        public async Task<List<WGPeerLastHandshake>> GetUsersWithHandshake()
        {
            var json = await SendRequestBase(RequestMethod.GET, Endpoints.WireguardPeers + $"?.proplist=last-handshake,name");
            var model = json.ToModel<List<WGPeerLastHandshake>>();
            return model
                .Where(u => !string.IsNullOrWhiteSpace(u.LastHandshake))
                .ToList();
        }

        public async Task<MTInfo> GetInfo()
        {
            var json = await SendGetRequestAsync(Endpoints.SystemResource);
            return json.ToModel<MTInfo>();
        }

        public async Task<MTIdentity> GetName()
        {
            var json = await SendGetRequestAsync(Endpoints.SystemIdentity);
            return json.ToModel<MTIdentity>();
        }

        public async Task<CreationStatus> SetName(MTIdentityUpdateModel identity) // Create Model
        {
            var itemJson = JObject.FromObject(identity, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }).ToString();
            var json = await SendPostRequestAsync(Endpoints.SystemIdentity + "/set", itemJson);
            return json == "[]" ? new()
            {
                Success = true,
                Item = await GetName()
            } : json.ToModel<CreationStatus>();
        }

        public async Task<LoginStatus> TryConnectAsync()
        {
            var connection = await SendGetRequestAsync(Endpoints.Empty, true);
            return connection.ToModel<LoginStatus>();
        }

        public async Task<List<ActiveUser>> GetActiveSessions()
        {
            var json = await SendGetRequestAsync($"{Endpoints.ActiveUsers}?name=" + MT_USER);
            return json.ToModel<List<ActiveUser>>();
        }

        public async Task<List<Job>> GetJobs()
        {
            var json = await SendGetRequestAsync(Endpoints.Jobs);
            return json.ToModel<List<Job>>();
        }

        public async Task<DNS> GetDNS()
        {
            var json = await SendGetRequestAsync(Endpoints.DNS);
            return json.ToModel<DNS>();
        }

        public async Task<CreationStatus> SetDNS(MTDNSUpdateModel dns) // Create Model
        {
            var itemJson = JObject.FromObject(dns, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }).ToString();
            var json = await SendPostRequestAsync(Endpoints.DNS + "/set", itemJson);
            return json == "[]" ? new()
            {
                Success = true,
                Item = await GetDNS()
            } : json.ToModel<CreationStatus>();
        }

        public async Task<string> KillJob(string JobID)
        {
            return await SendDeleteRequestAsync($"{Endpoints.Jobs}/" + JobID);
        }

        public async Task<CreationStatus> CreateServer(WGServerCreateModel server)
        {
            return await CreateItem<WGServer>(Endpoints.Wireguard, server);
        }

        public async Task<CreationStatus> CreateUser(WGPeerCreateModel user)
        {
            return await CreateItem<WGPeer>(Endpoints.WireguardPeers, user);
        }

        public async Task<CreationStatus> UpdateServer(WGServerUpdateModel server)
        {
            var itemJson = JObject.FromObject(server, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.Wireguard, itemJson, server.Id);
        }

        public async Task<CreationStatus> UpdateUser(WGPeerUpdateModel user)
        {
            var itemJson = JObject.FromObject(user, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.WireguardPeers, itemJson, user.Id);
        }

        public async Task<CreationStatus> SetServerEnabled(WGEnability enability)
        {
            return await UpdateItem(Endpoints.Wireguard, new { disabled = enability.Disabled }, enability.ID);
        }

        public async Task<CreationStatus> SetUserEnabled(WGEnability enability)
        {
            return await UpdateItem(Endpoints.WireguardPeers, new { disabled = enability.Disabled }, enability.ID);
        }

        public async Task<CreationStatus> DeleteServer(string id)
        {
            return await DeleteItem(Endpoints.Wireguard, id);
        }

        public async Task<CreationStatus> DeleteUser(string id)
        {
            return await DeleteItem(Endpoints.WireguardPeers, id);
        }

        public async Task<string> GetTrafficSpeed()
        {
            return await SendPostRequestAsync(Endpoints.MonitorTraffic, "{\"interface\":\"ether1\",\"duration\":\"3s\"}");
        }

        public async Task<List<Script>> GetScripts()
        {
            var json = await SendGetRequestAsync(Endpoints.Scripts);
            return json.ToModel<List<Script>>();
        }

        public async Task<CreationStatus> CreateScript(ScriptCreateModel script)
        {
            return await CreateItem<Script>(Endpoints.Scripts, script);
        }

        public async Task<CreationStatus> DeleteScript(string id)
        {
            return await DeleteItem(Endpoints.Scripts, id);
        }

        public async Task<CreationStatus> UpdateScript(ScriptUpdateModel script)
        {
            var itemJson = JObject.FromObject(script, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.Scripts, itemJson, script.Id);
        }

        public async Task<string> RunScript(string name)
        {
            return await SendPostRequestAsync(Endpoints.Execute, "{\"script\":\"" + name + "\"}");
        }

        public async Task<List<Scheduler>> GetSchedulers()
        {
            var json = await SendGetRequestAsync(Endpoints.Scheduler);
            return json.ToModel<List<Scheduler>>();
        }

        public async Task<Scheduler> GetScheduler(string id)
        {
            var json = await SendGetRequestAsync($"{Endpoints.Scheduler}/{id}");
            return json.ToModel<Scheduler>();
        }

        public async Task<Scheduler> GetSchedulerByName(string name)
        {
            var json = await SendGetRequestAsync($"{Endpoints.Scheduler}/{name}");
            return json.ToModel<Scheduler>();
        }

        public async Task<CreationStatus> CreateScheduler(SchedulerCreateModel scheduler)
        {
            return await CreateItem<Scheduler>(Endpoints.Scheduler, scheduler);
        }

        public async Task<CreationStatus> UpdateScheduler(SchedulerUpdateModel scheduler)
        {
            var itemJson = JObject.FromObject(scheduler, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.Scheduler, itemJson, scheduler.Id);
        }

        public async Task<CreationStatus> DeleteScheduler(string id)
        {
            return await DeleteItem(Endpoints.Scheduler, id);
        }

        public async Task<List<IPPool>> GetIPPools()
        {
            var json = await SendGetRequestAsync(Endpoints.IPPool);
            return json.ToModel<List<IPPool>>();
        }

        public async Task<CreationStatus> CreateIPPool(IPPoolCreateModel ipPool)
        {
            return await CreateItem<IPPool>(Endpoints.IPPool, ipPool);
        }

        public async Task<CreationStatus> UpdateIPPool(IPPoolUpdateModel ipPool)
        {
            var itemJson = JObject.FromObject(ipPool, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.IPPool, itemJson, ipPool.Id);
        }

        public async Task<CreationStatus> DeleteIPPool(string id)
        {
            return await DeleteItem(Endpoints.IPPool, id);
        }

        // Simple Queue
        public async Task<List<SimpleQueue>> GetSimpleQueues()
        {
            var json = await SendGetRequestAsync(Endpoints.Queue);
            return json.ToModel<List<SimpleQueue>>();
        }

        public async Task<SimpleQueue> GetSimpleQueueByName(string name)
        {
            var json = await SendGetRequestAsync($"{Endpoints.Queue}/{name}");
            return json.ToModel<SimpleQueue>();
        }

        public async Task<CreationStatus> CreateSimpleQueue(SimpleQueueCreateModel simpleQueue)
        {
            return await CreateItem<SimpleQueue>(Endpoints.Queue, simpleQueue);
        }

        public async Task<CreationStatus> UpdateSimpleQueue(SimpleQueueUpdateModel simpleQueue)
        {
            var itemJson = JObject.FromObject(simpleQueue, new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            });
            return await UpdateItem(Endpoints.Queue, itemJson, simpleQueue.Id);
        }

        public async Task<CreationStatus> DeleteSimpleQueue(string id)
        {
            return await DeleteItem(Endpoints.Queue, id);
        }

        public async Task<string> ResetSimpleQueue(string id)
        {
            return await SendPostRequestAsync($"{Endpoints.Queue}/reset-counters", $"{{\".id\":\"{id}\"}}");
        }

        // Base Methods
        private async Task<CreationStatus> CreateItem<T>(string Endpoint, object ItemCreateModel)
        {
            var jsonData = JObject.Parse(JsonConvert.SerializeObject(ItemCreateModel, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));
            var json = await SendPutRequestAsync(Endpoint, jsonData);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            T item = default;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                item = JsonConvert.DeserializeObject<T>(json);
            }
            else if (obj.TryGetValue("error", out var Error))
            {
                var error = JsonConvert.DeserializeObject<CreationStatus>(json);
                success = false;
                code = Error.Value<string>();
                message = error.Message;
                detail = error.Detail;
            }
            else
            {
                success = false;
                message = "Failed";
                detail = json;
            };
            return new()
            {
                Code = code,
                Message = message,
                Detail = detail,
                Success = success,
                Item = item ?? default
            };
        }

        private async Task<CreationStatus> DeleteItem(string Endpoint, string ItemID)
        {
            var json = await SendDeleteRequestAsync($"{Endpoint}/{ItemID}");
            if (string.IsNullOrWhiteSpace(json))
            {
                return new()
                {
                    Success = true
                };
            }
            else
            {
                return new()
                {
                    Success = false,
                    Item = json
                };
            }
        }

        private async Task<CreationStatus> UpdateItem<T>(string Endpoint, T item, string itemId)
        {
            var json = await SendPatchRequestAsync($"{Endpoint}/{itemId}", item);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            T itemType = default;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                itemType = JsonConvert.DeserializeObject<T>(json);
            }
            else if (obj.TryGetValue("error", out var Error))
            {
                var error = JsonConvert.DeserializeObject<CreationStatus>(json);
                success = false;
                code = Error.Value<string>();
                message = error.Message;
                detail = error.Detail;
            }
            else
            {
                success = false;
                message = "Failed";
                detail = json;
            };
            return new()
            {
                Code = code,
                Message = message,
                Detail = detail,
                Success = success,
                Item = itemType
            };
        }

        private async Task<string> SendRequestBase(RequestMethod Method, string Endpoint, object? Data = null, bool IsTest = false)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod(Method.ToString()), $"https://{MT_IP}/rest/{Endpoint}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            if (!IsTest) request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
            if (Data != null)
            {
                string content = (Data is string @string) ? @string : JsonConvert.SerializeObject(Data);
                request.Content = new StringContent(content);
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            }

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<string> SendGetRequestAsync(string URL, bool IsTest = false)
        {
            return await SendRequestBase(RequestMethod.GET, URL, IsTest: IsTest);
        }

        private async Task<string> SendPostRequestAsync(string URL, string Data)
        {
            return await SendRequestBase(RequestMethod.POST, URL, Data);
        }

        private async Task<string> SendDeleteRequestAsync(string URL)
        {
            return await SendRequestBase(RequestMethod.DELETE, URL);
        }

        private async Task<string> SendPutRequestAsync(string URL, object Data)
        {
            return await SendRequestBase(RequestMethod.PUT, URL, Data);
        }

        private async Task<string> SendPatchRequestAsync(string URL, object Data)
        {
            return await SendRequestBase(RequestMethod.PATCH, URL, Data);
        }
    }
}
