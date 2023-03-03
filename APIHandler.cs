using Microsoft.AspNetCore.Hosting.Server;
using MTWireGuard.Models;
using MTWireGuard.Models.Mikrotik;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace MTWireGuard
{
    public static class APIHandler
    {
        private static readonly string MT_IP = Environment.GetEnvironmentVariable("MT_IP");
        private static readonly string MT_USER = Environment.GetEnvironmentVariable("MT_USER");
        private static readonly string MT_PASS = Environment.GetEnvironmentVariable("MT_PASS");

        public static async Task<List<Log>> GetLogsAsync()
        {
            string json = await SendGetRequestAsync("log");
            return JsonConvert.DeserializeObject<List<Log>>(json);
        }

        public static async Task<List<WGServer>> GetServersAsync()
        {
            string json = await SendGetRequestAsync("interface/wireguard");
            return JsonConvert.DeserializeObject<List<WGServer>>(json);
        }

        public static async Task<WGServer> GetServer(string Name)
        {
            var servers = await GetServersAsync();
            return servers.Find(s => s.Name == Name);
        }

        public static async Task<List<ServerTraffic>> GetServersTraffic()
        {
            var json = await SendPostRequestAsync("interface", "{\"stats\", {\".proplist\":\"name, type, rx-byte, tx-byte\"}}");
            return JsonConvert.DeserializeObject<List<ServerTraffic>>(json);
        }

        public static async Task<List<WGPeer>> GetUsersAsync()
        {
            using var db = new DBContext();
            string json = await SendGetRequestAsync("interface/wireguard/peers");
            return JsonConvert.DeserializeObject<List<WGPeer>>(json);
        }

        public static async Task<WGPeer> GetUser(string id)
        {
            var users = await GetUsersAsync();
            return users.Find(u => u.Id == id);
        }

        public static async Task<MTInfo> GetInfo()
        {
            var json = await SendGetRequestAsync("system/resource");
            return JsonConvert.DeserializeObject<MTInfo>(json);
        }

        public static async Task<MTIdentity> GetName()
        {
            var json = await SendGetRequestAsync("system/identity");
            return JsonConvert.DeserializeObject<MTIdentity>(json);
        }

        public static async Task<LoginStatus> TryConnectAsync()
        {
            var connection = await SendGetRequestAsync("", true);
            return JsonConvert.DeserializeObject<LoginStatus>(connection);
        }

        public static async Task<List<ActiveUser>> GetActiveSessions()
        {
            var json = await SendGetRequestAsync("user/active?name=" + MT_USER);
            return JsonConvert.DeserializeObject<List<ActiveUser>>(json);
        }

        public static async Task<List<Job>> GetJobs()
        {
            var json = await SendGetRequestAsync("system/script/job");
            return JsonConvert.DeserializeObject<List<Job>>(json);
        }

        public static async Task<string> KillJob(string JobID)
        {
            return await SendDeleteRequestAsync("system/script/job/" + JobID);
        }

        public static async Task<CreationStatus> CreateServer(WGServerCreateModel server)
        {
            var json = await SendPutRequestAsync("interface/wireguard", server);
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

        public static async Task<CreationStatus> CreateUser(WGPeerCreateModel user)
        {
            var jsonData = JObject.Parse(JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));

            var json = await SendPutRequestAsync("interface/wireguard/peers", jsonData);
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

        public static async Task<CreationStatus> UpdateServer(WGServerUpdateModel server)
        {
            var serverJson = JObject.Parse(JsonConvert.SerializeObject(server, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));
            var json = await SendPatchRequestAsync($"interface/wireguard/{server.Id}", serverJson);
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

        public static async Task<CreationStatus> UpdateUser(WGPeerUpdateModel user)
        {
            var userJson = JObject.Parse(JsonConvert.SerializeObject(user, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Ignore
            }));
            var json = await SendPatchRequestAsync($"interface/wireguard/peers/{user.Id}", userJson);
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

        public static async Task<CreationStatus> SetServerEnabled(WGEnability enability)
        {
            var json = await SendPatchRequestAsync($"interface/wireguard/{enability.ID}", new { disabled = enability.Disabled });
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

        public static async Task<CreationStatus> SetUserEnabled(WGEnability enability)
        {
            var json = await SendPatchRequestAsync($"interface/wireguard/peers/{enability.ID}", new { disabled = enability.Disabled });
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

        public static async Task<CreationStatus> DeleteServer(string id)
        {
            var json = await SendDeleteRequestAsync("interface/wireguard/" + id);
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

        public static async Task<CreationStatus> DeleteUser(string id)
        {
            var json = await SendDeleteRequestAsync("interface/wireguard/peers/" + id);
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

        public static async Task<string> GetTrafficSpeed()
        {
            return await SendPostRequestAsync("interface/monitor-traffic", "{\"interface\":\"ether1\",\"duration\":\"3s\"}");
        }

        private static async Task<string> SendGetRequestAsync(string URL, bool IsTest = false)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = delegate { return true; }
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://{MT_IP}/rest/{URL}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            if (!IsTest) request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> SendPostRequestAsync(string URL, string Data)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://{MT_IP}/rest/{URL}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            request.Content = new StringContent(Data);
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> SendDeleteRequestAsync(string URL)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod("DELETE"), $"https://{MT_IP}/rest/{URL}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> SendPutRequestAsync(string URL, object Data)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod("PUT"), $"https://{MT_IP}/rest/{URL}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");
            
            request.Content = new StringContent(JsonConvert.SerializeObject(Data));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> SendPatchRequestAsync(string URL, object Data)
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true
            };
            using HttpClient httpClient = new(handler);
            using var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"https://{MT_IP}/rest/{URL}");
            string base64authorization = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{MT_USER}:{MT_PASS}"));
            request.Headers.TryAddWithoutValidation("Authorization", $"Basic {base64authorization}");

            request.Content = new StringContent(JsonConvert.SerializeObject(Data));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            HttpResponseMessage response = await httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}
