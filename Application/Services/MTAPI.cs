using AutoMapper;
using MikrotikAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using QRCoder;

namespace MTWireGuard.Application.Services
{
    public class MTAPI : IMikrotikRepository
    {
        private readonly IMapper mapper;
        private readonly DBContext dbContext;
        private readonly APIWrapper wrapper;
        private bool disposed = false;
        public MTAPI(IMapper mapper, DBContext dbContext)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;

            string MT_IP = Environment.GetEnvironmentVariable("MT_IP");
            string MT_USER = Environment.GetEnvironmentVariable("MT_USER");
            string MT_PASS = Environment.GetEnvironmentVariable("MT_PASS");
            this.wrapper = new(MT_IP, MT_USER, MT_PASS);
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
        public async Task<string> GetUserTunnelConfig(int id)
        {
            WGPeerViewModel User = await GetUser(id);
            WGServerViewModel Server = await GetServer(User.Interface);
            string IP = Environment.GetEnvironmentVariable("MT_PUBLIC_IP"),
                Endpoint = Server != null ? $"{IP}:{Server.ListenPort}" : "";
            return $"[Interface]{Environment.NewLine}" +
                $"Address = {User.Address ?? "0.0.0.0/0"}{Environment.NewLine}" +
                $"PrivateKey = {User.PrivateKey}{Environment.NewLine}" +
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
        public async Task<MTIdentityViewModel> GetName()
        {
            var model = await wrapper.GetName();
            return mapper.Map<MTIdentityViewModel>(model);
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
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> CreateUser(UserCreateModel peer)
        {
            var user = mapper.Map<MikrotikAPI.Models.WGPeerCreateModel>(peer);
            var model = await wrapper.CreateUser(user);
            if (model.Success)
            {
                var item = model.Item as MikrotikAPI.Models.WGPeer;
                await dbContext.Users.AddAsync(new()
                {
                     Id = Convert.ToInt32(item.Id[1..], 16),
                     Name = peer.Name,
                     PrivateKey = peer.PrivateKey,
                     PublicKey = peer.PublicKey
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
            var mtPeer = mapper.Map<MikrotikAPI.Models.WGPeerUpdateModel>(user);
            var mtUpdate = await wrapper.UpdateUser(mtPeer);
            if (mtUpdate.Success)
            {
                var exists = await dbContext.Users.FindAsync(user.Id);
                dbContext.ChangeTracker.Clear();
                if (exists != null)
                {
                    dbContext.Users.Update(new()
                    {
                        Id = user.Id,
                        Name = user.Name ?? exists.Name,
                        PrivateKey = user.PrivateKey ?? exists.PrivateKey,
                        PublicKey = user.PublicKey ?? exists.PublicKey
                    });
                }
                else
                    await dbContext.Users.AddAsync(new()
                    {
                        Id = user.Id,
                        Name = user.Name,
                        PublicKey = user.PublicKey,
                        PrivateKey = user.PrivateKey
                    });
                await dbContext.SaveChangesAsync();
            }
            return mapper.Map<CreationResult>(mtUpdate);
        }

        public async Task<CreationResult> UpdateServer(ServerUpdateModel server)
        {
            var srv = mapper.Map<MikrotikAPI.Models.WGServerUpdateModel>(server);
            var mtUpdate = await wrapper.UpdateServer(srv);
            return mapper.Map<CreationResult>(mtUpdate);
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
            return mapper.Map<CreationResult>(delete);
        }
        public async Task<CreationResult> DeleteUser(int id)
        {
            var delete = await wrapper.DeleteUser($"*{id:X}");
            return mapper.Map<CreationResult>(delete);
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
                //
            }

            // Free any unmanaged objects here.
            //
            disposed = true;
        }
    }
}
