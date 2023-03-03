using MTWireGuard.Models;
using MTWireGuard.Models.Mikrotik;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MTWireGuard
{
    public static class MTAPIHandler
    {
        private static readonly string MT_IP = Environment.GetEnvironmentVariable("MT_IP");
        private static readonly string MT_USER = Environment.GetEnvironmentVariable("MT_USER");
        private static readonly string MT_PASS = Environment.GetEnvironmentVariable("MT_PASS");

        public static async Task<List<LogViewModel>> GetLogsAsync()
        {
            string json = await SendGetRequestAsync("log");
            var model = JsonConvert.DeserializeObject<List<Log>>(json);
            return model.Select(x => new LogViewModel()
            {
                Id = Convert.ToUInt64(x.Id[1..], 16),
                Message = x.Message,
                Time = x.Time,
                Topics = x.Topics.Split(',').Select(t => t = Helper.UpperCaseTopics.Contains(t) ? t.ToUpper() : t.FirstCharToUpper()).ToList()
            }).ToList();
        }

        public static async Task<List<WGServerViewModel>> GetServersAsync()
        {
            string json = await SendGetRequestAsync("interface/wireguard");
            var model = JsonConvert.DeserializeObject<List<WGServer>>(json);
            return model.Select(x => new WGServerViewModel()
            {
                Id = Convert.ToInt32(x.Id[1..], 16),
                Name = x.Name,
                ListenPort = Convert.ToUInt16(x.ListenPort),
                MTU = Convert.ToUInt16(x.MTU),
                Running = x.Running,
                IsEnabled = !x.Disabled,
                PublicKey = x.PublicKey,
                PrivateKey = x.PrivateKey
            }).ToList();
        }

        public static async Task<WGServerViewModel> GetServer(string Name)
        {
            var servers = await GetServersAsync();
            return servers.Find(s => s.Name == Name);
        }

        public static async Task<List<ServerTraffic>> GetServersTraffic()
        {
            var json = await SendPostRequestAsync("interface", "{\"stats\", {\".proplist\":\"name,rx-byte, tx-byte\"}}");
            List<ServerTraffic> traffic = JsonConvert.DeserializeObject<List<ServerTraffic>>(json);
            return traffic;
        }

        public static async Task<List<WGPeerViewModel>> GetUsersAsync()
        {
            using var db = new DBContext();
            string json = await SendGetRequestAsync("interface/wireguard/peers");
            List<WGPeer> apiUsers = JsonConvert.DeserializeObject<List<WGPeer>>(json);
            List<WGPeerDBModel> dbUsers = db.Users.ToList();
            Dictionary<int, bool> differences = new();
            // Start Checking DB and Router sync
            foreach (var apiUser in apiUsers)
            {
                var id = Convert.ToInt32(apiUser.Id[1..], 16);
                var dbUser = dbUsers.Find(u => u.Id == id);
                if (dbUser == null)
                {
                    differences.Add(id, true);
                }
                else
                {
                    string publickey = apiUser.PublicKey;
                    bool publicKeyDifferent = dbUser.PublicKey != publickey;
                    bool noPrivateKey = string.IsNullOrWhiteSpace(dbUser.PrivateKey);
                    differences.Add(id, publicKeyDifferent | noPrivateKey);
                }
            }
            // End Checking
            return apiUsers.Select(x => new WGPeerViewModel()
            {
                Id = Convert.ToInt32(x.Id[1..], 16),
                Name = (dbUsers.Find(u => u.Id == Convert.ToInt32(x.Id[1..], 16)) != null) ? dbUsers.Find(u => u.Id == Convert.ToInt32(x.Id[1..], 16)).Name : "",
                Address = x.AllowedAddress,
                CurrentAddress = $"{x.CurrentEndpointAddress}:{x.CurrentEndpointPort}",
                Interface = x.Interface,
                IsEnabled = !x.Disabled,
                PublicKey = x.PublicKey,
                Download = x.RX,
                Upload = x.TX,
                IsDifferent = differences[Convert.ToInt32(x.Id[1..], 16)]
            }).ToList();
        }

        public static async Task<WGPeerViewModel> GetUser(int id)
        {
            var users = await GetUsersAsync();
            return users.Find(u => u.Id == id);
        }

        public static async Task<string> GetIPAddress()
        {
            var json = await SendGetRequestAsync("ip/address?interface=ether1&.proplist=address");
            var IP = JsonConvert.DeserializeObject<List<EtherIP>>(json).FirstOrDefault();
            var address = IP.Address[..IP.Address.IndexOf('/')];
            return address;
        }

        public static async Task<MTInfoViewModel> GetInfo()
        {
            var json = await SendGetRequestAsync("system/resource");
            var model = JsonConvert.DeserializeObject<MTInfo>(json);
            return new()
            {
                Architecture = model.ArchitectureName,
                BoardName = model.BoardName,
                CPU = model.CPU,
                CPUCount = Convert.ToByte(model.CPUCount),
                CPUFrequency = Convert.ToInt16(model.CPUFrequency),
                CPULoad = Convert.ToByte(model.CPULoad),
                TotalHDDBytes = Convert.ToInt64(model.TotalHDDSpace),
                FreeHDDBytes = Convert.ToInt64(model.FreeHDDSpace),
                UsedHDDBytes = Convert.ToInt64(model.TotalHDDSpace) - Convert.ToInt64(model.FreeHDDSpace),
                FreeHDDPercentage = (byte)(Convert.ToInt64(model.FreeHDDSpace) * 100 / Convert.ToInt64(model.TotalHDDSpace)),
                TotalRAMBytes = Convert.ToInt64(model.TotalMemory),
                FreeRAMBytes = Convert.ToInt64(model.FreeMemory),
                UsedRAMBytes = Convert.ToInt64(model.TotalMemory) - Convert.ToInt64(model.FreeMemory),
                FreeRAMPercentage = (byte)(Convert.ToInt64(model.FreeMemory) * 100 / Convert.ToInt64(model.TotalMemory)),
                TotalHDD = Helper.ConvertByteSize(Convert.ToInt64(model.TotalHDDSpace)),
                FreeHDD = Helper.ConvertByteSize(Convert.ToInt64(model.FreeHDDSpace)),
                UsedHDD = Helper.ConvertByteSize(Convert.ToInt64(model.TotalHDDSpace) - Convert.ToInt64(model.FreeHDDSpace)),
                TotalRAM = Helper.ConvertByteSize(Convert.ToInt64(model.TotalMemory)),
                FreeRAM = Helper.ConvertByteSize(Convert.ToInt64(model.FreeMemory)),
                UsedRAM = Helper.ConvertByteSize(Convert.ToInt64(model.TotalMemory) - Convert.ToInt64(model.FreeMemory)),
                UPTime = model.Uptime.
        Replace('d', ' ').
        Replace('h', ':').
        Replace('m', ':').
        Replace("s", ""),
                Platform = model.Platform,
                Version = model.Version
            };
        }

        public static async Task<bool> TryConnectAsync()
        {
            try
            {
                var connection = await SendGetRequestAsync("", true);
                var status = JsonConvert.DeserializeObject<LoginStatus>(connection);
                if ((status.Error == 400 && status.Message == "Bad Request") || (status.Error == 401 && status.Message == "Unauthorized"))
                {
                    return true;
                }
                throw new($"[{status.Error}] Login failed, {status.Message}.<br>Enter router username/password in environment variables (MT_USER/MT_PASS).");
            }
            catch
            {
                throw;
            }
        }

        public static async Task<string> GetCurrentSessionID()
        {
            var sessionJson = await SendGetRequestAsync("user/active?name=admin&via=api&.proplist=when");
            JArray session = JArray.Parse(sessionJson);
            var jobsJson = await SendGetRequestAsync("system/script/job?type=api-login&started=" + session.FirstOrDefault()["when"]);
            JArray jobs = JArray.Parse(jobsJson);
            return jobs.FirstOrDefault()[".id"].ToString();
        }

        public static async Task<string> KillJob(string JobID)
        {
            return await SendDeleteRequestAsync("system/script/job/" + JobID);
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
            string APIResponse = await response.Content.ReadAsStringAsync();
            return APIResponse;
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
    }
}
