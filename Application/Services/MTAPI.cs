using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MikrotikAPI;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Utils;
using NetTools;
using Newtonsoft.Json;
using QRCoder;
using Serilog;
using System.Net.WebSockets;
using System.Text;

namespace MTWireGuard.Application.Services
{
    public class MTAPI : IMikrotikRepository
    {
        private readonly IMapper mapper;
        private readonly DBContext dbContext;
        private readonly APIWrapper wrapper;
        private readonly ILogger logger;
        private readonly string MT_IP, MT_USER, MT_PASS;
        private bool disposed = false;
        public MTAPI(IMapper mapper, DBContext dbContext, ILogger logger)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            this.logger = logger;

            MT_IP = Environment.GetEnvironmentVariable("MT_IP");
            MT_USER = Environment.GetEnvironmentVariable("MT_USER");
            MT_PASS = Environment.GetEnvironmentVariable("MT_PASS");
            wrapper = new(MT_IP, MT_USER, MT_PASS);
        }
        public async Task<List<LogViewModel>> GetLogsAsync()
        {
            var model = await wrapper.GetLogsAsync();
            var result = mapper.Map<List<LogViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<List<WGServerViewModel>> GetServersAsync()
        {
            var model = await wrapper.GetServersAsync();
            var result = mapper.Map<List<WGServerViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<WGServerViewModel> GetServer(string Name)
        {
            var model = await wrapper.GetServer(Name);
            return mapper.Map<WGServerViewModel>(model);
        }
        public async Task<WGServerViewModel> GetServer(int id)
        {
            var model = await wrapper.GetServerById(ConverterUtil.ParseEntityID(id));
            return mapper.Map<WGServerViewModel>(model);
        }

        public async Task<List<ServerTrafficViewModel>> GetServersTraffic()
        {
            var model = await wrapper.GetServersTraffic();
            return mapper.Map<List<ServerTrafficViewModel>>(model);
        }
        public async Task<WGServerStatistics> GetServersCount()
        {
            var servers = await wrapper.GetServersAsync();
            return new()
            {
                Total = servers.Count,
                Active = servers.Count(s => !s.Disabled),
                Running = servers.Count(s => s.Running)
            };
        }
        public async Task<List<WGPeerViewModel>> GetUsersAsync()
        {
            var model = await wrapper.GetUsersAsync();
            var result = mapper.Map<List<WGPeerViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<WGPeerViewModel> GetUser(int id)
        {
            var model = await wrapper.GetUser(ConverterUtil.ParseEntityID(id));
            return mapper.Map<WGPeerViewModel>(model);
        }
        public async Task<string> GetUserHandshake(string id)
        {
            var model = await wrapper.GetUserHandshake(id);
            string input = model.LastHandshake;
            if (input == null) return "never";
            var ts = ConverterUtil.ConvertToTimeSpan(input);
            return ts.ToString();
        }
        public async Task<List<WGPeerLastHandshakeViewModel>> GetUsersHandshakes()
        {
            var model = await wrapper.GetUsersWithHandshake();
            return mapper.Map<List<WGPeerLastHandshakeViewModel>>(model);
        }
        public async Task<WGUserStatistics> GetUsersCount()
        {
            var users = await wrapper.GetUsersAsync();
            var runningUsers = await GetUsersHandshakes();
            var onlines = CoreUtil.FilterOnlineUsers(runningUsers);
            return new()
            {
                Total = users.Count,
                Active = users.Count(s => !s.Disabled),
                Online = onlines.Count
            };
        }
        public async Task<string> GetUserTunnelConfig(int id)
        {
            WGPeerViewModel User = await GetUser(id);
            WGServerViewModel Server = await GetServer(User.Interface);
            DNS MTDNS = await GetDNS();
            string IP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP"),
                Endpoint = Server != null ? $"{IP}:{Server.ListenPort}" : "",
                DNS = !User.InheritDNS ? User.DNSAddress : Server?.DNSAddress ?? MTDNS.Servers;
            return $"[Interface]{Environment.NewLine}" +
                $"Address = {User.IPAddress ?? "0.0.0.0/0"}{Environment.NewLine}" +
                $"PrivateKey = {User.PrivateKey}{Environment.NewLine}" +
                $"DNS = {DNS}" +
                $"{Environment.NewLine}" +
                $"[Peer]{Environment.NewLine}" +
                $"AllowedIPs = {User.AllowedIPs}{Environment.NewLine}" +
                $"Endpoint = {Endpoint}{Environment.NewLine}" +
                $"PublicKey = {Server?.PublicKey ?? ""}";
        }
        public async Task<string> GetUserV2rayQRCodeBase64(int id)
        {
            WGPeerViewModel User = await GetUser(id);
            WGServerViewModel Server = await GetServer(User.Interface);
            string IP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP"),
                Endpoint = Server != null ? $"{IP}:{Server.ListenPort}" : "";

            string config = $"wireguard://{User.PrivateKey}@{Endpoint}?address={User.IPAddress ?? "0.0.0.0/0"}&publickey={Server?.PublicKey ?? ""}&mtu={Server?.MTU ?? 1420}#{User.Name}";

            using QRCodeGenerator qrGenerator = new();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(config, QRCodeGenerator.ECCLevel.Q);
            using PngByteQRCode qrCode = new(qrCodeData);
            var QR = qrCode.GetGraphic(20);
            return Convert.ToBase64String(QR);
        }
        public async Task<CreationResult> ImportUsers(List<UserImportModel> users, WebSocket socket)
        {
            var existingUsers = await GetUsersAsync();
            int total = users.Count, inserted = 0, updated = 0, warning = 0, failed = 0, processed = 0;
            foreach (var user in users)
            {
                bool built = false;
                // send state to client
                int progress = (int)((++processed) / (double)total * 100);

                // Send progress update to WebSocket
                if (socket.State == WebSocketState.Open)
                {
                    var progressMessage = new { progress };
                    var progressJson = System.Text.Json.JsonSerializer.Serialize(progressMessage);
                    var progressBytes = Encoding.UTF8.GetBytes(progressJson);
                    var arraySegment = new ArraySegment<byte>(progressBytes, 0, progressBytes.Length);
                    await socket.SendAsync(arraySegment,
                                        WebSocketMessageType.Text,
                                        true,
                                        CancellationToken.None);
                }
                else
                {
                    logger.Error("Socket Issue", socket);
                }

                int userId = 0;
                var existing = existingUsers.Find(u => u.PublicKey == user.PublicKey);
                if (existing != null) // user exists in system
                {
                    userId = existing.Id;
                    var update = await UpdateUser(new UserUpdateModel()
                    {
                        Id = userId,
                        AllowedAddress = user.AllowedAddress,
                        AllowedIPs = user.AllowedIPs,
                        DNSAddress = user.DNSAddress,
                        Expire = DateTime.Parse(user.Expire),
                        Interface = user.Interface,
                        IPAddress = user.IPAddress,
                        Name = user.Name,
                        PrivateKey = user.PrivateKey,
                        PublicKey = user.PublicKey,
                        Traffic = user.Traffic,
                        Bandwidth = user.Bandwidth.Replace("B", string.Empty).Replace(" ", string.Empty),
                    });
                    built = update.Code == "200";
                    if (built) updated++;
                    else
                    {
                        failed++;
                        continue;
                    }
                }
                else
                {
                    var add = await CreateUser(new UserCreateModel()
                    {
                        AllowedAddress = user.AllowedAddress,
                        AllowedIPs = user.AllowedIPs,
                        DNSAddress = user.DNSAddress,
                        Expire = DateTime.Parse(user.Expire),
                        Interface = user.Interface,
                        IPAddress = user.IPAddress,
                        Name = user.Name,
                        PrivateKey = user.PrivateKey,
                        PublicKey = user.PublicKey,
                        Traffic = user.Traffic,
                        Disabled = !user.Enabled
                    });
                    built = add.Code == "200";
                    if (built) inserted++;
                    else
                    {
                        failed++;
                        continue;
                    }
                }
                if (built)
                {
                    var mtUser = await wrapper.GetUserByPublicKey(user.PublicKey);
                    var dbUser = mtUser != null ? dbContext.Users.FirstOrDefault(u => u.Id == ConverterUtil.ParseEntityID(mtUser.Id)) : null;
                    var userTraffic = mtUser != null ? dbContext.LastKnownTraffic.FirstOrDefault(t => t.UserID == ConverterUtil.ParseEntityID(mtUser.Id)) : null;
                    if (dbUser != null)
                    {
                        dbUser.TX = user.UploadBytes;
                        dbUser.RX = user.DownloadBytes;

                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        logger.Error("user #{uid}{user} doesn't exist on DB!", ConverterUtil.ParseEntityID(mtUser.Id), user);
                        warning++;
                    }
                    if (userTraffic != null)
                    {
                        userTraffic.TX = user.UploadBytes;
                        userTraffic.RX = user.DownloadBytes;

                        await dbContext.SaveChangesAsync();
                    }
                    else
                    {
                        logger.Error("user #{uid}{user} doesn't exist on traffic DB!", ConverterUtil.ParseEntityID(mtUser.Id), user);
                        warning++;
                    }
                }
            }
            // Send finish message to WebSocket
            if (socket.State == WebSocketState.Open)
            {
                bool succeed = true;
                var progressMessage = new { succeed };
                var progressJson = System.Text.Json.JsonSerializer.Serialize(progressMessage);
                var progressBytes = Encoding.UTF8.GetBytes(progressJson);
                var arraySegment = new ArraySegment<byte>(progressBytes, 0, progressBytes.Length);
                await socket.SendAsync(arraySegment,
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);
            }
            else
            {
                logger.Warning("Socket Issue", socket);
            }
            return new()
            {
                Code = "200",
                Title = "Import users!",
                Description = $"Imported: {inserted}<br>Updated: {updated}<hr>Warning: {warning}<br>Failed: {failed}<hr>Total: {total}"
            };
        }
        public async Task<string> GetQRCodeBase64(int id)
        {
            string config = await GetUserTunnelConfig(id);

            using QRCodeGenerator qrGenerator = new();
            using QRCodeData qrCodeData = qrGenerator.CreateQrCode(config, QRCodeGenerator.ECCLevel.Q);
            using PngByteQRCode qrCode = new(qrCodeData);
            var QR = qrCode.GetGraphic(20);
            return Convert.ToBase64String(QR);
        }
        public async Task<MTInfoViewModel> GetInfo()
        {
            var model = await wrapper.GetInfo();
            return mapper.Map<MTInfoViewModel>(model);
        }
        public async Task<IdentityViewModel> GetName()
        {
            var model = await wrapper.GetName();
            return mapper.Map<IdentityViewModel>(model);
        }
        public async Task<CreationResult> SetName(IdentityUpdateModel identity)
        {
            var mtIdentity = mapper.Map<MikrotikAPI.Models.MTIdentityUpdateModel>(identity);
            var model = await wrapper.SetName(mtIdentity);
            return mapper.Map<CreationResult>(model);
        }
        public async Task<bool> TryConnectAsync()
        {
            try
            {
                var model = await wrapper.TryConnectAsync();
                if ((model.Error == 400 && model.Message == "Bad Request") || (model.Error == 401 && model.Message == "Unauthorized"))
                {
                    return true;
                }
                throw new($"[{model.Error}] Login failed, {model.Message}.<br>Enter router username/password in environment variables (MT_USER/MT_PASS).");
            }
            catch (Exception ex)
            {
                throw new($"Login failed, {ex.Message}");
            }
        }
        public async Task<List<ActiveUserViewModel>> GetActiveSessions()
        {
            var model = await wrapper.GetActiveSessions();
            return mapper.Map<List<ActiveUserViewModel>>(model);
        }
        public async Task<List<JobViewModel>> GetJobs()
        {
            var model = await wrapper.GetJobs();
            return mapper.Map<List<JobViewModel>>(model);
        }
        public async Task<string> GetCurrentSessionID()
        {
            var activeSessions = await wrapper.GetActiveSessions();
            var apiSession = activeSessions.Find(x => x.Via == "api");
            var jobs = await wrapper.GetJobs();
            var currentJob = jobs.Find(x => x.Started == apiSession.When);
            return currentJob.Id;
        }

        public async Task<string> KillJob(string JobID)
        {
            return await wrapper.KillJob(JobID);
        }

        public async Task<CreationResult> CreateServer(ServerCreateModel server)
        {
            var srv = mapper.Map<MikrotikAPI.Models.WGServerCreateModel>(server);
            var model = await wrapper.CreateServer(srv);
            if (model.Success)
            {
                var addIP = await wrapper.CreateIPAddress(new()
                {
                    Address = server.IPAddress,
                    Interface = server.Name
                });
                if (addIP.Success)
                {
                    var item = model.Item as WGServer;
                    var serverId = ConverterUtil.ParseEntityID(item.Id);
                    var mtDNS = (await GetDNS()).Servers;
                    await dbContext.Servers.AddAsync(new()
                    {
                        Id = serverId,
                        InheritDNS = server.InheritDNS,
                        DNSAddress = server.InheritDNS ? mtDNS : server.DNSAddress,
                        IPPoolId = server.IPPoolId,
                        UseIPPool = server.UseIPPool
                    });
                    await dbContext.SaveChangesAsync();
                }
                else
                    return mapper.Map<CreationResult>(addIP);
            }
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> CreateUser(UserCreateModel peer)
        {
            var user = mapper.Map<MikrotikAPI.Models.WGPeerCreateModel>(peer);
            var usedIPs = (await GetUsersAsync()).Where(u => u.Interface == peer.Interface).Select(u => u.IPAddress);
            var server = await GetServer(peer.Interface);
            string ipAddress = peer.IPAddress ?? "0.0.0.0/0";
            if (peer.InheritIP && !string.IsNullOrWhiteSpace(server.IPPool))
            {
                var range = IPAddressRange.Parse(server.IPPool);
                foreach (var ip in range)
                {
                    if (ip.ToString() == range.Begin.ToString() || ip.ToString() == range.End.ToString()) continue; // Skip Network and Broadcast address
                    if (server.IPAddress.Contains(ip.ToString())) continue; // Skip Gateway address
                    if (usedIPs.Contains($"{ip}/32")) continue; // Skip if IP is used previously
                    ipAddress = ip.ToString();
                    break;
                }
            }
            user.ClientAddress = ipAddress;
            //get dns
            var inheritDNS = peer.InheritDNS;
            string mtDns = (await GetDNS()).Servers;
            string dnsAddress = (!inheritDNS) ? peer.DNSAddress : server.DNSAddress ?? string.Join(mtDns, ',');
            //end dns
            //allowed address
            user.AllowedAddress = peer.InheritAllowedAddress ? ipAddress : (peer.AllowedAddress ?? "0.0.0.0/0");
            //allowed address
            var model = await wrapper.CreateUser(user);
            if (model.Success)
            {
                var item = model.Item as WGPeer;
                var userID = ConverterUtil.ParseEntityID(item.Id);
                var deleteScheduler = peer.Expire != new DateTime() && peer.Expire != null ? await CreateScheduler(new()
                {
                    Name = $"DisableUser{userID}",
                    Policies = ["read", "write", "test", "sensitive"],
                    OnEvent = Constants.UserExpirationScript(ConverterUtil.ParseEntityID(userID)),
                    StartDate = DateOnly.FromDateTime((DateTime)peer.Expire),
                    StartTime = TimeOnly.FromDateTime((DateTime)peer.Expire),
                    Comment = $"Disable Wireguard Peer: {peer.Name}",
                    Interval = TimeSpan.Zero
                }) : null;
                if (deleteScheduler == null || deleteScheduler.Code == "200")
                {
                    var userQueue = string.IsNullOrEmpty(peer.Bandwidth) ? null : await CreateSimpleQueue(new()
                    {
                        Name = $"QueueUser{userID}",
                        MaxLimit = peer.Bandwidth,
                        Target = ipAddress,
                        Comment = $"Bandwidth controller Wireguard Peer: {peer.Name}"
                    });
                    if (userQueue == null || userQueue.Code == "200")
                    {
                        await dbContext.Users.AddAsync(new()
                        {
                            Id = userID,
                            TrafficLimit = peer.Traffic,
                            AllowedIPs = peer.AllowedIPs ?? "0.0.0.0/0",
                            InheritDNS = inheritDNS,
                            InheritIP = peer.InheritIP
                        });
                        await dbContext.LastKnownTraffic.AddAsync(new()
                        {
                            UserID = userID,
                            RX = 0,
                            TX = 0,
                            CreationTime = DateTime.Now
                        });
                        await dbContext.SaveChangesAsync();
                    }
                    else if (userQueue != null)
                    {
                        var deleteUser = await DeleteUser(userID);
                        logger.Error("Failed to create queue with code: {code}, title: {title}, description: {desc}", userQueue.Code, userQueue.Title, userQueue.Description);
                        return new()
                        {
                            Code = "500",
                            Title = "Failed to create user",
                            Description = "Failed to create bandwidth queue"
                        };
                    }
                }
                else if (deleteScheduler != null)
                {
                    var deleteUser = await DeleteUser(userID);
                    logger.Error("Failed to create scheduler with code: {code}, title: {title}, description: {desc}", deleteScheduler.Code, deleteScheduler.Title, deleteScheduler.Description);
                    return new()
                    {
                        Code = "500",
                        Title = "Failed to create user",
                        Description = "Failed to create expire script"
                    };
                }
            }
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> UpdateUser(UserUpdateModel user)
        {
            var mtPeer = mapper.Map<MikrotikAPI.Models.WGPeerUpdateModel>(user);
            //allowed address
            string ipAddress = user.InheritAllowedAddress ? (await GetUser(user.Id)).IPAddress : user.IPAddress;
            mtPeer.AllowedAddress = user.InheritAllowedAddress ? ipAddress : (user.AllowedAddress ?? "0.0.0.0/0");
            //allowed address
            var mtUpdate = await wrapper.UpdateUser(mtPeer);
            if (mtUpdate.Success)
            {
                var scheduler = await GetSchedulerByName($"DisableUser{user.Id}");
                var queue = await GetSimpleQueueByName($"QueueUser{user.Id}");
                var exists = await dbContext.Users.FindAsync(user.Id);
                dbContext.ChangeTracker.Clear();
                var schedulerId = scheduler?.Id ?? 0;
                var queueId = queue?.Id ?? 0;
                if (user.Expire != new DateTime())
                {
                    var deleteScheduler = scheduler == null ?
                        await CreateScheduler(new()
                        {
                            Name = $"DisableUser{user.Id}",
                            Policies = ["read", "write", "test", "sensitive"],
                            OnEvent = Constants.UserExpirationScript(ConverterUtil.ParseEntityID(user.Id)),
                            StartDate = DateOnly.FromDateTime(user.Expire),
                            StartTime = TimeOnly.FromDateTime(user.Expire),
                            Comment = $"Disable Wireguard Peer: {user.Name}",
                            Interval = TimeSpan.Zero
                        }) :
                        await UpdateScheduler(new()
                        {
                            Id = scheduler.Id,
                            StartDate = DateOnly.FromDateTime(user.Expire),
                            StartTime = TimeOnly.FromDateTime(user.Expire),
                            Interval = TimeSpan.Zero
                        });
                    if (deleteScheduler.Code == "200")
                    {
                        scheduler = await GetSchedulerByName($"DisableUser{user.Id}");
                        schedulerId = scheduler.Id;
                    }
                    else
                    {
                        logger.Error("Failed to create/update scheduler with code: {code}, title: {title}, description: {desc}", deleteScheduler.Code, deleteScheduler.Title, deleteScheduler.Description);
                    }
                }
                var expireID = (user.Expire != new DateTime()) ? schedulerId : 0;
                if (!string.IsNullOrEmpty(user.Bandwidth))
                {
                    var userQueue = queue == null ?
                        await CreateSimpleQueue(new()
                        {
                            Name = $"QueueUser{user.Id}",
                            MaxLimit = user.Bandwidth,
                            Target = ipAddress,
                            Comment = $"Bandwidth controller Wireguard Peer: {user.Name}"
                        }) :
                        await UpdateSimpleQueue(new()
                        {
                            Id = ConverterUtil.ParseEntityID(queue.Id),
                            MaxLimit = user.Bandwidth,
                            Target = ipAddress,
                            Comment = $"Bandwidth controller Wireguard Peer: {user.Name}"
                        });
                    if (userQueue.Code == "200")
                    {
                        queue = await GetSimpleQueueByName($"QueueUser{user.Id}");
                        queueId = queue.Id;
                    }
                    else
                    {
                        logger.Error("Failed to create/update queue with code: {code}, title: {title}, description: {desc}", userQueue.Code, userQueue.Title, userQueue.Description);
                    }
                }
                if (exists != null)
                {
                    dbContext.Users.Update(new()
                    {
                        Id = user.Id,
                        InheritDNS = user.InheritDNS,
                        InheritIP = user.InheritIP,
                        TrafficLimit = user.Traffic,
                        AllowedIPs = user.AllowedIPs ?? "0.0.0.0/0"
                    });
                }
                else
                {
                    dbContext.ChangeTracker.Clear();
                    await dbContext.Users.AddAsync(new()
                    {
                        Id = user.Id,
                        InheritDNS = user.InheritDNS,
                        InheritIP = user.InheritIP,
                        TrafficLimit = user.Traffic,
                        AllowedIPs = user.AllowedIPs ?? "0.0.0.0/0"
                    });
                    await dbContext.LastKnownTraffic.AddAsync(new()
                    {
                        UserID = user.Id,
                        RX = 0,
                        TX = 0,
                        CreationTime = DateTime.Now
                    });
                }
                // Remove scheduler if expiration disabled
                if (schedulerId > 0 && user.Expire == new DateTime())
                {
                    await wrapper.DeleteScheduler(ConverterUtil.ParseEntityID(schedulerId));
                }
                await dbContext.SaveChangesAsync();
            }
            return mapper.Map<CreationResult>(mtUpdate);
        }

        public async Task<CreationResult> UpdateServer(ServerUpdateModel server)
        {
            var srv = mapper.Map<MikrotikAPI.Models.WGServerUpdateModel>(server);
            var model = await wrapper.UpdateServer(srv);
            if (model.Success)
            {
                var exists = await dbContext.Servers.FindAsync(server.Id);
                dbContext.ChangeTracker.Clear();
                var serverIP = await wrapper.GetServerIPAddress(server.Name);
                var ipChanged = !serverIP.Select(ip => ip.Address).Contains(server.IPAddress);
                bool ipChangeState = true;
                if (ipChanged)
                {
                    var changeIP = serverIP.Any() ?
                        await wrapper.UpdateIPAddress(new()
                        {
                            Id = serverIP.Find(ip => ip.Interface == server.Name).Id,
                            Address = server.IPAddress
                        }) :
                        await wrapper.CreateIPAddress(new()
                        {
                            Address = server.IPAddress,
                            Interface = server.Name
                        });
                    ipChangeState = changeIP.Success;
                }
                if (ipChangeState)
                {
                    var mtDNS = (await GetDNS()).Servers;
                    if (exists == null) // Server not exists in DB
                    {
                        await dbContext.Servers.AddAsync(new()
                        {
                            Id = server.Id,
                            InheritDNS = server.InheritDNS,
                            DNSAddress = server.InheritDNS ? mtDNS : server.DNSAddress,
                            IPPoolId = server.IPPoolId,
                            UseIPPool = server.UseIPPool
                        });
                    }
                    else
                    {
                        dbContext.Servers.Update(new()
                        {
                            Id = server.Id,
                            InheritDNS = server.InheritDNS,
                            DNSAddress = !server.InheritDNS && server.DNSAddress != null ? server.DNSAddress : exists.DNSAddress,
                            IPPoolId = server.IPPoolId,
                            UseIPPool = server.UseIPPool
                        });
                    }
                    await dbContext.SaveChangesAsync();
                }
            }
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> ImportServers(List<ServerImportModel> servers, WebSocket socket)
        {
            var existingServers = await GetServersAsync();
            int total = servers.Count, inserted = 0, updated = 0, warning = 0, failed = 0, processed = 0;
            foreach (var server in servers)
            {
                bool built = false;
                // send state to client
                int progress = (int)((++processed) / (double)total * 100);

                // Send progress update to WebSocket
                if (socket.State == WebSocketState.Open)
                {
                    var progressMessage = new { progress };
                    var progressJson = System.Text.Json.JsonSerializer.Serialize(progressMessage);
                    var progressBytes = Encoding.UTF8.GetBytes(progressJson);
                    var arraySegment = new ArraySegment<byte>(progressBytes, 0, progressBytes.Length);
                    await socket.SendAsync(arraySegment,
                                        WebSocketMessageType.Text,
                                        true,
                                        CancellationToken.None);
                }
                else
                {
                    logger.Error("Socket Issue", socket);
                }

                int serverId = 0;
                var existing = existingServers.Find(u => u.PrivateKey == server.PrivateKey);
                if (server.UseIPPool)
                {
                    if (!string.IsNullOrEmpty(server.IPPool) && server.IPPoolId == 0) // IPPool not exists
                    {
                        var addPool = await CreateIPPool(new()
                        {
                            Name = $"{server.Name}Pool",
                            Ranges = server.IPPool
                        });
                        if (addPool.Code != "200")
                        {
                            logger.Error("IP Pool {IPPool} can't be created! {error}, {description}", server.IPPool, addPool.Title, addPool.Description);
                            warning++;
                        }
                        else
                        {
                            var pools = await GetIPPools(); // to be checked
                            server.IPPoolId = pools.FirstOrDefault(p => p.Name == $"{server.Name}Pool").Id;
                        }
                    }
                    else // IPPool exists (maybe updated panel)
                    {
                        var pools = await GetIPPools();
                        var pool = pools.FirstOrDefault(p => p.Id == server.IPPoolId);
                        server.IPPoolId = pool.Id;
                        logger.Warning("IP Pool exists {pool}", pool);
                    }
                }
                if (existing != null) // server exists in system
                {
                    serverId = existing.Id;
                    var update = await UpdateServer(new ServerUpdateModel()
                    {
                        Id = serverId,
                        Name = server.Name,
                        IPAddress = server.IPAddress,
                        DNSAddress = server.DNSAddress,
                        PrivateKey = server.PrivateKey,
                        MTU = server.MTU,
                        ListenPort = server.ListenPort,
                        UseIPPool = server.UseIPPool,
                        IPPoolId = server.IPPoolId
                    });
                    built = update.Code == "200";
                    if (built) updated++;
                    else
                    {
                        failed++;
                        continue;
                    }
                }
                else
                {
                    var add = await CreateServer(new ServerCreateModel()
                    {
                        DNSAddress = server.DNSAddress,
                        IPAddress = server.IPAddress,
                        Name = server.Name,
                        PrivateKey = server.PrivateKey,
                        Enabled = server.Enabled,
                        ListenPort = server.ListenPort.ToString(),
                        MTU = server.MTU.ToString(),
                        UseIPPool = server.UseIPPool,
                        IPPoolId = server.IPPoolId
                    });
                    built = add.Code == "200";
                    if (built) inserted++;
                    else
                    {
                        failed++;
                        continue;
                    }
                }
            }
            // Send finish message to WebSocket
            if (socket.State == WebSocketState.Open)
            {
                bool succeed = true;
                var progressMessage = new { succeed };
                var progressJson = System.Text.Json.JsonSerializer.Serialize(progressMessage);
                var progressBytes = Encoding.UTF8.GetBytes(progressJson);
                var arraySegment = new ArraySegment<byte>(progressBytes, 0, progressBytes.Length);
                await socket.SendAsync(arraySegment,
                                    WebSocketMessageType.Text,
                                    true,
                                    CancellationToken.None);
            }
            else
            {
                logger.Warning("Socket Issue", socket);
            }
            return new()
            {
                Code = "200",
                Title = "Import users!",
                Description = $"Imported: {inserted}<br>Updated: {updated}<hr>Failed: {failed}<hr>Total: {total}"
            };
        }

        public async Task<CreationResult> EnableServer(int id)
        {
            var enable = await wrapper.SetServerEnabled(new()
            {
                ID = ConverterUtil.ParseEntityID(id),
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableServer(int id)
        {
            var enable = await wrapper.SetServerEnabled(new()
            {
                ID = ConverterUtil.ParseEntityID(id),
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> EnableUser(int id)
        {
            var enable = await wrapper.SetUserEnabled(new()
            {
                ID = ConverterUtil.ParseEntityID(id),
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableUser(int id)
        {
            var enable = await wrapper.SetUserEnabled(new()
            {
                ID = ConverterUtil.ParseEntityID(id),
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DeleteServer(int id)
        {
            var server = await GetServer(id);
            var ips = await GetServerIP(server.Name);
            var delete = await wrapper.DeleteServer(ConverterUtil.ParseEntityID(id));
            if (delete.Success)
            {
                logger.ForContext("EntityUpdate", true)
                  .Information("Operation: {Operation}, Entity: {Entity}, Status: {result}, Time: {Timestamp}",
                      "DELETE",
                      JsonConvert.SerializeObject(server),
                      new
                      {
                          Code = delete.Code,
                          Message = delete.Message,
                          Detail = delete.Detail,
                      }, DateTime.UtcNow);
                var dbServer = await dbContext.Servers.FindAsync(id);
                if (dbServer != null)
                {
                    dbContext.Servers.Remove(dbServer);
                    await dbContext.SaveChangesAsync();
                }
                foreach (var ip in ips)
                {
                    var deleteIP = await DeleteIPAddress(ip.Id);
                    logger.ForContext("EntityUpdate", true)
                      .Information("Operation: {Operation}, Entity: {Entity}, Status: {result}, Time: {Timestamp}",
                          "DELETE",
                          JsonConvert.SerializeObject(ip),
                          new
                          {
                              Code = deleteIP.Code,
                              Message = deleteIP.Title,
                              Detail = deleteIP.Description,
                          }, DateTime.UtcNow);
                }
            }
            return mapper.Map<CreationResult>(delete);
        }

        public async Task<CreationResult> DeleteUser(int id)
        {
            var delete = await wrapper.DeleteUser(ConverterUtil.ParseEntityID(id));
            if (delete.Success)
            {
                var user = await dbContext.Users.FindAsync(id);
                var scheduler = await GetSchedulerByName($"DisableUser{id}");
                var queue = await GetSimpleQueueByName($"QueueUser{id}");
                if (scheduler != null) await wrapper.DeleteScheduler(ConverterUtil.ParseEntityID(scheduler.Id));
                if (queue != null) await wrapper.DeleteSimpleQueue(ConverterUtil.ParseEntityID(queue.Id));
                if (user != null) dbContext.Users.Remove(user);
                await dbContext.LastKnownTraffic.Where(t => t.UserID == id).ExecuteDeleteAsync();
                await dbContext.DataUsages.Where(d => d.UserID == id).ExecuteDeleteAsync();
                await dbContext.SaveChangesAsync();
            }
            return mapper.Map<CreationResult>(delete);
        }

        public async Task<List<ScriptViewModel>> GetScripts()
        {
            var model = await wrapper.GetScripts();
            return mapper.Map<List<ScriptViewModel>>(model);
        }

        public async Task<CreationResult> CreateScript(Models.Mikrotik.ScriptCreateModel script)
        {
            var scr = mapper.Map<MikrotikAPI.Models.ScriptCreateModel>(script);
            var model = await wrapper.CreateScript(scr);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<string> RunScript(string name)
        {
            return await wrapper.RunScript(name);
        }

        public async Task<List<SchedulerViewModel>> GetSchedulers()
        {
            var model = await wrapper.GetSchedulers();
            return mapper.Map<List<SchedulerViewModel>>(model);
        }

        public async Task<SchedulerViewModel> GetScheduler(int id)
        {
            var model = await wrapper.GetScheduler(ConverterUtil.ParseEntityID(id));
            return mapper.Map<SchedulerViewModel>(model);
        }

        public async Task<SchedulerViewModel> GetSchedulerByName(string name)
        {
            var model = await wrapper.GetSchedulerByName(name);
            return mapper.Map<SchedulerViewModel>(model);
        }

        public async Task<CreationResult> CreateScheduler(Models.Mikrotik.SchedulerCreateModel scheduler)
        {
            var sched = mapper.Map<MikrotikAPI.Models.SchedulerCreateModel>(scheduler);
            var model = await wrapper.CreateScheduler(sched);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> UpdateScheduler(Models.Mikrotik.SchedulerUpdateModel scheduler)
        {
            var sched = Map<MikrotikAPI.Models.SchedulerUpdateModel>(scheduler);
            var model = await wrapper.UpdateScheduler(sched);
            return Map<CreationResult>(model);
        }

        public async Task<DNS> GetDNS()
        {
            return await wrapper.GetDNS();
        }

        public async Task<CreationResult> SetDNS(DNSUpdateModel dns)
        {
            var mtDNS = mapper.Map<MikrotikAPI.Models.MTDNSUpdateModel>(dns);
            var model = await wrapper.SetDNS(mtDNS);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<List<IPPoolViewModel>> GetIPPools()
        {
            var model = await wrapper.GetIPPools();
            return mapper.Map<List<IPPoolViewModel>>(model);
        }

        public async Task<CreationResult> CreateIPPool(PoolCreateModel ipPool)
        {
            var pool = mapper.Map<IPPoolCreateModel>(ipPool);
            var model = await wrapper.CreateIPPool(pool);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> UpdateIPPool(PoolUpdateModel ipPool)
        {
            var pool = mapper.Map<IPPoolUpdateModel>(ipPool);
            var model = await wrapper.UpdateIPPool(pool);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> DeleteIPPool(int id)
        {
            var delete = await wrapper.DeleteIPPool(ConverterUtil.ParseEntityID(id));
            return mapper.Map<CreationResult>(delete);
        }

        public async Task<List<IPAddressViewModel>> GetIPAddresses()
        {
            var model = await wrapper.GetIPAddresses();
            return mapper.Map<List<IPAddressViewModel>>(model);
        }

        public async Task<List<IPAddressViewModel>> GetServerIP(string Name)
        {
            var model = await wrapper.GetServerIPAddress(Name);
            return mapper.Map<List<IPAddressViewModel>>(model);
        }

        public async Task<CreationResult> DeleteIPAddress(int id)
        {
            var delete = await wrapper.DeleteIP(ConverterUtil.ParseEntityID(id));
            return mapper.Map<CreationResult>(delete);
        }

        public async Task<CreationResult> ResetUserTraffic(int id)
        {
            try
            {
                var traffics = dbContext.DataUsages.Where(t => t.UserID == id);
                dbContext.DataUsages.RemoveRange(traffics);

                var lastKnownTraffic = dbContext.LastKnownTraffic.FirstOrDefault(t => t.UserID == id);
                if (lastKnownTraffic != null)
                {
                    lastKnownTraffic.RX = 0;
                    lastKnownTraffic.TX = 0;
                }

                var user = dbContext.Users.Find(id);
                if (user != null)
                {
                    user.RX = 0;
                    user.TX = 0;
                }

                await dbContext.SaveChangesAsync();
                return new()
                {
                    Code = "200",
                    Description = "User traffic usage has been reset.",
                    Title = "Traffic reset done"
                };
            }
            catch (Exception ex)
            {
                logger.Error("Reset user traffic", ex);
                return new()
                {
                    Code = "500",
                    Description = ex.Message,
                    Title = "Traffic reset failed!"
                };
            }
        }

        // Simple Queue
        public async Task<List<SimpleQueueViewModel>> GetSimpleQueues()
        {
            var model = await wrapper.GetSimpleQueues();
            return Map<List<SimpleQueueViewModel>>(model);
        }

        public async Task<SimpleQueueViewModel> GetSimpleQueueByName(string name)
        {
            var model = await wrapper.GetSimpleQueueByName(name);
            return string.IsNullOrEmpty(model.Id) ? null : Map<SimpleQueueViewModel>(model);
        }

        public async Task<CreationResult> CreateSimpleQueue(SimpleQueueCreateModel simpleQueue)
        {
            var queue = Map<SimpleQueueCreateModel>(simpleQueue);
            var model = await wrapper.CreateSimpleQueue(queue);
            return Map<CreationResult>(model);
        }

        public async Task<CreationResult> UpdateSimpleQueue(SimpleQueueUpdateModel simpleQueue)
        {
            var queue = Map<SimpleQueueUpdateModel>(simpleQueue);
            var model = await wrapper.UpdateSimpleQueue(queue);
            return Map<CreationResult>(model);
        }

        public async Task<CreationResult> DeleteSimpleQueue(int id)
        {
            var delete = await wrapper.DeleteSimpleQueue(ConverterUtil.ParseEntityID(id));
            return Map<CreationResult>(delete);
        }

        // General
        private T Map<T>(object source)
        {
            try
            {
                return mapper.Map<T>(source);
            }
            catch (AutoMapperMappingException autoMapperException)
            {
                logger.Error(autoMapperException, "Error mapping");
                throw;
            }
            catch (Exception ex)
            {
                logger.Error(ex, $"Failed to map {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                dbContext.Dispose();
            }

            disposed = true;
        }
    }
}
