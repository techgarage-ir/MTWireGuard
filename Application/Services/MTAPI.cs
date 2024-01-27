using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Caching.Memory;
using MikrotikAPI;
using MikrotikAPI.Models;
using MTWireGuard.Application.MinimalAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using NetTools;
using QRCoder;
using System;
using System.Security.Principal;

namespace MTWireGuard.Application.Services
{
    public class MTAPI : IMikrotikRepository
    {
        private readonly IMapper mapper;
        private readonly DBContext dbContext;
        private readonly IMemoryCache memoryCache;
        private readonly APIWrapper wrapper;
        private readonly string MT_IP, MT_USER, MT_PASS;
        private bool disposed = false;
        public MTAPI(IMapper mapper, DBContext dbContext, IMemoryCache memoryCache)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
            this.memoryCache = memoryCache;

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
        public async Task<List<ServerTrafficViewModel>> GetServersTraffic() {
            var model = await wrapper.GetServersTraffic();
            return mapper.Map<List<ServerTrafficViewModel>>(model);
        }
        public async Task<List<WGPeerViewModel>> GetUsersAsync()
        {
            var model = await wrapper.GetUsersAsync();
            var result = mapper.Map<List<WGPeerViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<WGPeerViewModel> GetUser(int id)
        {
            var model = await wrapper.GetUser($"*{id:X}");
            return mapper.Map<WGPeerViewModel>(model);
        }
        public async Task<string> GetUserHandshake(string id)
        {
            var model = await wrapper.GetUserHandshake(id);
            string input = model.LastHandshake;
            if (input == null) return "never";
            var ts = Helper.ConvertToTimeSpan(input);
            return ts.ToString();
        }
        public async Task<string> GetUserTunnelConfig(int id)
        {
            WGPeerViewModel User = await GetUser(id);
            WGServerViewModel Server = await GetServer(User.Interface);
            string IP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP"),
                Endpoint = Server != null ? $"{IP}:{Server.ListenPort}" : "",
                DNS = !User.InheritDNS ? User.DNSAddress : Server.DNSAddress;
            return $"[Interface]{Environment.NewLine}" +
                $"Address = {User.Address ?? "0.0.0.0/0"}{Environment.NewLine}" +
                $"PrivateKey = {User.PrivateKey}{Environment.NewLine}" +
                $"DNS = {DNS}" +
                $"{Environment.NewLine}" +
                $"[Peer]{Environment.NewLine}" +
                $"AllowedIPs = 0.0.0.0/0{Environment.NewLine}" +
                $"Endpoint = {Endpoint}{Environment.NewLine}" +
                $"PublicKey = {Server?.PublicKey ?? ""}";
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
            catch(Exception ex)
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
                    var serverId = Helper.ParseEntityID(item.Id);
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
            var usedIPs = (await GetUsersAsync()).Where(u => u.Interface == peer.Interface).Select(u => u.Address);
            var server = await GetServer(peer.Interface);
            string allowedAddress = "0.0.0.0/0";
            if (peer.InheritIP && !string.IsNullOrWhiteSpace(server.IPPool))
            {
                var range = IPAddressRange.Parse(server.IPPool);
                foreach (var ip in range)
                {
                    if (ip.ToString() == range.Begin.ToString() || ip.ToString() == range.End.ToString()) continue; // Skip Network and Broadcast address
                    if (server.IPAddress.Contains(ip.ToString())) continue; // Skip Gateway address
                    if (usedIPs.Contains($"{ip}/32")) continue; // Skip if IP is used previously
                    allowedAddress = ip.ToString();
                    break;
                }
            }
            user.AllowedAddress = allowedAddress;
            //get dns
            var inheritDNS = peer.InheritDNS;
            string mtDns = (await GetDNS()).Servers;
            string dnsAddress = (!inheritDNS) ? peer.DNSAddress : server.DNSAddress ?? string.Join(mtDns, ',');
            //end dns
            var model = await wrapper.CreateUser(user);
            if (model.Success)
            {
                var item = model.Item as MikrotikAPI.Models.WGPeer;
                var userID = Convert.ToInt32(item.Id[1..], 16);
                var expireID = (peer.Expire != new DateTime() && peer.Expire != null) ? HangfireManager.SetUserExpiration(userID, (DateTime)peer.Expire) : 0;
                await dbContext.Users.AddAsync(new()
                {
                    Id = userID,
                    Name = peer.Name,
                    PrivateKey = peer.PrivateKey,
                    PublicKey = peer.PublicKey,
                    Expire = peer.Expire,
                    ExpireID = expireID,
                    TrafficLimit = peer.Traffic,
                    DNSAddress = dnsAddress,
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
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> SyncUser(UserSyncModel user)
        {
            CreationResult result = new();
            var userID = user.Id;
            var dbUser = await dbContext.Users.FindAsync(userID);
            var mtUser = await GetUser(userID);
            if (dbUser == null)
            {
                await dbContext.Users.AddAsync(new()
                {
                    Id = userID,
                    Name = user.Name,
                    PublicKey = user.PublicKey,
                    PrivateKey = user.PrivateKey
                });
                var lkt = await dbContext.LastKnownTraffic.FindAsync(userID);
                if (lkt == null)
                    await dbContext.LastKnownTraffic.AddAsync(new()
                    {
                        UserID = userID,
                        RX = 0,
                        TX = 0,
                        CreationTime = DateTime.Now
                    });
                await dbContext.SaveChangesAsync();
                result = new()
                {
                    Code = "200",
                    Title = "Success",
                    Description = "Database updated successfully."
                };
            }
            else if (dbUser.PublicKey != user.PublicKey)
            {
                var fxUser = dbUser;
                fxUser.Name = user.Name;
                fxUser.PrivateKey = user.PrivateKey;
                fxUser.PublicKey = user.PublicKey;
                dbContext.Users.Update(fxUser);
                await dbContext.SaveChangesAsync();
                result = new()
                {
                    Code = "200",
                    Title = "Success",
                    Description = "Database updated successfully."
                };
            }
            if (mtUser.PublicKey != user.PublicKey)
            {
                var fxUser = mapper.Map<MikrotikAPI.Models.WGPeerUpdateModel>(user);
                var update = await wrapper.UpdateUser(fxUser);
                result = mapper.Map<CreationResult>(update);
            }
            return result;
        }

        public async Task<CreationResult> UpdateUser(UserUpdateModel user)
        {
            var mtPeer = mapper.Map<MikrotikAPI.Models.WGPeerUpdateModel>(user.);
            var mtUpdate = await wrapper.UpdateUser(mtPeer);
            if (mtUpdate.Success)
            {
                var exists = await dbContext.Users.FindAsync(user.Id);
                dbContext.ChangeTracker.Clear();
                var expireID = (user.Expire != new DateTime()) ? HangfireManager.SetUserExpiration(user.Id, user.Expire) : 0;
                if (exists != null)
                {
                    dbContext.Users.Update(new()
                    {
                        Id = user.Id,
                        Name = user.Name ?? exists.Name,
                        PrivateKey = user.PrivateKey ?? exists.PrivateKey,
                        PublicKey = user.PublicKey ?? exists.PublicKey,
                        Expire = user.Expire,
                        ExpireID = expireID,
                        InheritDNS = user.InheritDNS,
                        DNSAddress = user.DNSAddress,
                        InheritIP = user.InheritIP,
                        TrafficLimit = user.Traffic
                    });
                }
                else
                {
                    dbContext.ChangeTracker.Clear();
                    await dbContext.Users.AddAsync(new()
                    {
                        Id = user.Id,
                        Name = user.Name,
                        PublicKey = user.PublicKey,
                        PrivateKey = user.PrivateKey,
                        Expire = user.Expire,
                        ExpireID = expireID,
                        InheritDNS = user.InheritDNS,
                        DNSAddress = user.DNSAddress,
                        InheritIP = user.InheritIP,
                        TrafficLimit = user.Traffic
                    });
                    await dbContext.LastKnownTraffic.AddAsync(new()
                    {
                        UserID = user.Id,
                        RX = 0,
                        TX = 0,
                        CreationTime = DateTime.Now
                    });
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

        public async Task<CreationResult> EnableServer(int id)
        {
            var enable = await wrapper.SetServerEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableServer(int id)
        {
            var enable = await wrapper.SetServerEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> EnableUser(int id)
        {
            var enable = await wrapper.SetUserEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableUser(int id)
        {
            var enable = await wrapper.SetUserEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }
        
        public async Task<CreationResult> DeleteServer(int id)
        {
            var delete = await wrapper.DeleteServer($"*{id:X}");
            if (delete.Success)
            {
                var server = await dbContext.Servers.FindAsync(id);
                if (server != null)
                {
                    dbContext.Servers.Remove(server);
                    await dbContext.SaveChangesAsync();
                }
            }
            return mapper.Map<CreationResult>(delete);
        }
        public async Task<CreationResult> DeleteUser(int id)
        {
            var delete = await wrapper.DeleteUser($"*{id:X}");
            if (delete.Success)
            {
                var user = await dbContext.Users.FindAsync(id);
                if (user != null)
                {
                    dbContext.Users.Remove(user);
                    await dbContext.SaveChangesAsync();
                }
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
            /*var model = await wrapper.CreateScript(new MikrotikAPI.Models.ScriptCreateModel()
            {
                DontRequiredPermissions=false,
                Name = "SendActivityUpdates",
                Policy = "read,write,test",
                Source = Helper.PeersLastHandshakeScript($"https://{MT_IP}:7220/api/activity")
            });
            return mapper.Map<CreationResult>(model);*/
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

        public async Task<CreationResult> CreateScheduler(Models.Mikrotik.SchedulerCreateModel scheduler)
        {
            var sched = mapper.Map<MikrotikAPI.Models.SchedulerCreateModel>(scheduler);
            var model = await wrapper.CreateScheduler(sched);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<DNS> GetDNS()
        {
            return await wrapper.GetDNS();
        }

        public async Task<CreationResult> SetDNS(DNSUpdateModel dns)
        {
            var mtDNS= mapper.Map<MikrotikAPI.Models.MTDNSUpdateModel>(dns);
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
            var delete = await wrapper.DeleteIPPool($"*{id:X}");
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
                // Free any other managed objects here.
                dbContext.Dispose();
                memoryCache.Dispose();
            }

            // Free any unmanaged objects here.
            disposed = true;
        }
    }
}
