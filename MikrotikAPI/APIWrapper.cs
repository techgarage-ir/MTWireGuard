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
            var servers = await GetServersAsync();
            return servers.Find(s => s.Name == Name);
        }

        public async Task<List<ServerTraffic>> GetServersTraffic()
        {
            var json = await SendRequestBase(RequestMethod.GET, Endpoints.Interface, "{\"stats\", {\".proplist\":\"name, type, rx-byte, tx-byte\"}}");
            return json.ToModel<List<ServerTraffic>>();
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

        public async Task<string> KillJob(string JobID)
        {
            return await SendDeleteRequestAsync($"{Endpoints.Jobs}/" + JobID);
        }

        public async Task<CreationStatus> CreateServer(WGServerCreateModel server)
        {
            var json = await SendPutRequestAsync(Endpoints.Wireguard, server);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
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
                Success = success
            };
        }

        public async Task<CreationStatus> CreateUser(WGPeerCreateModel user)
        {
            var jsonData = JObject.Parse(JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));

            var json = await SendPutRequestAsync(Endpoints.WireguardPeers, jsonData);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            WGPeer peer = null;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                peer = JsonConvert.DeserializeObject<WGPeer>(json);
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
                Item = peer ?? null
            };
        }

        public async Task<CreationStatus> UpdateServer(WGServerUpdateModel server)
        {
            var serverJson = JObject.Parse(JsonConvert.SerializeObject(server, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));
            var json = await SendPatchRequestAsync($"{Endpoints.Wireguard}/{server.Id}", serverJson);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            WGServer srv = null;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                srv = JsonConvert.DeserializeObject<WGServer>(json);
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
                Item = srv ?? null
            };
        }

        public async Task<CreationStatus> UpdateUser(WGPeerUpdateModel user)
        {
            var userJson = JObject.Parse(JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));
            var json = await SendPatchRequestAsync($"{Endpoints.WireguardPeers}/{user.Id}", userJson);
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            WGPeer peer = null;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                peer = JsonConvert.DeserializeObject<WGPeer>(json);
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
                Item = peer ?? null
            };
        }

        public async Task<CreationStatus> SetServerEnabled(WGEnability enability)
        {
            var json = await SendPatchRequestAsync($"{Endpoints.Wireguard}/{enability.ID}", new { disabled = enability.Disabled });
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            WGPeer peer = null;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                peer = JsonConvert.DeserializeObject<WGPeer>(json);
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
                Item = peer ?? null
            };
        }

        public async Task<CreationStatus> SetUserEnabled(WGEnability enability)
        {
            var json = await SendPatchRequestAsync($"{Endpoints.WireguardPeers}/{enability.ID}", new { disabled = enability.Disabled });
            var obj = JObject.Parse(json);
            bool success = false;
            string code = string.Empty, message = string.Empty, detail = string.Empty;
            WGPeer peer = null;
            if (obj.TryGetValue(".id", out var Id))
            {
                success = true;
                peer = JsonConvert.DeserializeObject<WGPeer>(json);
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
                Item = peer ?? null
            };
        }

        public async Task<CreationStatus> DeleteServer(string id)
        {
            var json = await SendDeleteRequestAsync($"{Endpoints.Wireguard}/" + id);
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

        public async Task<CreationStatus> DeleteUser(string id)
        {
            var json = await SendDeleteRequestAsync($"{Endpoints.WireguardPeers}/" + id);
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

        public async Task<string> GetTrafficSpeed()
        {
            return await SendPostRequestAsync(Endpoints.MonitorTraffic, "{\"interface\":\"ether1\",\"duration\":\"3s\"}");
        }

        private async Task<string> SendRequestBase(RequestMethod Method, string Endpoint, object Data = null, bool IsTest = false)
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
