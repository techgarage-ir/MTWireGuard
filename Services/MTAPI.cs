using AutoMapper;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.EntityFrameworkCore;
using MTWireGuard.Models;
using MTWireGuard.Models.Mikrotik;
using MTWireGuard.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QRCoder;

namespace MTWireGuard.Services
{
    public class MTAPI : IMikrotikRepository
    {
        private readonly IMapper mapper;
        private readonly DBContext dbContext;
        private bool disposed = false;
        public MTAPI(IMapper mapper, DBContext dbContext)
        {
            this.mapper = mapper;
            this.dbContext = dbContext;
        }
        public async Task<List<LogViewModel>> GetLogsAsync()
        {
            var model = await APIHandler.GetLogsAsync();
            var result = mapper.Map<List<LogViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<List<WGServerViewModel>> GetServersAsync()
        {
            var model = await APIHandler.GetServersAsync();
            var result = mapper.Map<List<WGServerViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<WGServerViewModel> GetServer(string Name)
        {
            var model = await APIHandler.GetServer(Name);
            return mapper.Map<WGServerViewModel>(model);
        }
        public async Task<List<ServerTrafficViewModel>> GetServersTraffic() {
            var model = await APIHandler.GetServersTraffic();
            return mapper.Map<List<ServerTrafficViewModel>>(model);
        }
        public async Task<List<WGPeerViewModel>> GetUsersAsync()
        {
            var model = await APIHandler.GetUsersAsync();
            var result = mapper.Map<List<WGPeerViewModel>>(model);
            return result.OrderBy(list => list.Id).ToList();
        }
        public async Task<WGPeerViewModel> GetUser(int id)
        {
            var model = await APIHandler.GetUser($"*{id:X}");
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
            var model = await APIHandler.GetInfo();
            return mapper.Map<MTInfoViewModel>(model);
        }
        public async Task<MTIdentityViewModel> GetName()
        {
            var model = await APIHandler.GetName();
            return mapper.Map<MTIdentityViewModel>(model);
        }
        public async Task<bool> TryConnectAsync()
        {
            try
            {
                var model = await APIHandler.TryConnectAsync();
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
            var model = await APIHandler.GetActiveSessions();
            return mapper.Map<List<ActiveUserViewModel>>(model);
        }
        public async Task<List<JobViewModel>> GetJobs()
        {
            var model = await APIHandler.GetJobs();
            return mapper.Map<List<JobViewModel>>(model);
        }
        public async Task<string> GetCurrentSessionID()
        {
            var activeSessions = await APIHandler.GetActiveSessions();
            var apiSession = activeSessions.Find(x => x.Via == "api");
            var jobs = await APIHandler.GetJobs();
            var currentJob = jobs.Find(x => x.Started == apiSession.When);
            return currentJob.Id;
        }

        public async Task<string> KillJob(string JobID)
        {
            return await APIHandler.KillJob(JobID);
        }

        public async Task<CreationResult> CreateServer(ServerCreateModel server)
        {
            var srv = mapper.Map<WGServerCreateModel>(server);
            var model = await APIHandler.CreateServer(srv);
            return mapper.Map<CreationResult>(model);
        }

        public async Task<CreationResult> CreateUser(UserCreateModel peer)
        {
            var user = mapper.Map<WGPeerCreateModel>(peer);
            var model = await APIHandler.CreateUser(user);
            if (model.Success)
            {
                var item = model.Item as WGPeer;
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
                var fxUser = mapper.Map<WGPeerUpdateModel>(user);
                var update = await APIHandler.UpdateUser(fxUser);
                result = mapper.Map<CreationResult>(update);
            }
            return result;
        }

        public async Task<CreationResult> UpdateUser(UserUpdateModel user)
        {
            var mtPeer = mapper.Map<WGPeerUpdateModel>(user);
            var mtUpdate = await APIHandler.UpdateUser(mtPeer);
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
            var srv = mapper.Map<WGServerUpdateModel>(server);
            var mtUpdate = await APIHandler.UpdateServer(srv);
            return mapper.Map<CreationResult>(mtUpdate);
        }

        public async Task<CreationResult> EnableServer(int id)
        {
            var enable = await APIHandler.SetServerEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableServer(int id)
        {
            var enable = await APIHandler.SetServerEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> EnableUser(int id)
        {
            var enable = await APIHandler.SetUserEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = false
            });
            return mapper.Map<CreationResult>(enable);
        }

        public async Task<CreationResult> DisableUser(int id)
        {
            var enable = await APIHandler.SetUserEnabled(new()
            {
                ID = $"*{id:X}",
                Disabled = true
            });
            return mapper.Map<CreationResult>(enable);
        }
        
        public async Task<CreationResult> DeleteServer(int id)
        {
            var delete = await APIHandler.DeleteServer($"*{id:X}");
            return mapper.Map<CreationResult>(delete);
        }
        public async Task<CreationResult> DeleteUser(int id)
        {
            var delete = await APIHandler.DeleteUser($"*{id:X}");
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
