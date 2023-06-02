using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MikrotikAPI.Models;

namespace MTWireGuard.Application.Repositories
{
    public interface IMikrotikRepository : IDisposable
    {
        Task<List<LogViewModel>> GetLogsAsync();
        Task<List<WGServerViewModel>> GetServersAsync();
        Task<WGServerViewModel> GetServer(string Name);
        Task<List<ServerTrafficViewModel>> GetServersTraffic();
        Task<List<WGPeerViewModel>> GetUsersAsync();
        Task<WGPeerViewModel> GetUser(int id);
        Task<string> GetUserTunnelConfig(int id);
        Task<string> GetQRCodeBase64(int id);
        Task<MTInfoViewModel> GetInfo();
        Task<MTIdentityViewModel> GetName();
        Task<bool> TryConnectAsync();
        Task<List<ActiveUserViewModel>> GetActiveSessions();
        Task<List<JobViewModel>> GetJobs();
        Task<string> GetCurrentSessionID();
        Task<string> KillJob(string JobID);
        Task<CreationResult> CreateServer(ServerCreateModel server);
        Task<CreationResult> CreateUser(UserCreateModel peer);
        Task<CreationResult> SyncUser(UserSyncModel user);
        Task<CreationResult> UpdateUser(UserUpdateModel user);
        Task<CreationResult> UpdateServer(ServerUpdateModel server);
        Task<CreationResult> EnableServer(int id);
        Task<CreationResult> DisableServer(int id);
        Task<CreationResult> EnableUser(int id);
        Task<CreationResult> DisableUser(int id);
        Task<CreationResult> DeleteServer(int id);
        Task<CreationResult> DeleteUser(int id);
    }
}
