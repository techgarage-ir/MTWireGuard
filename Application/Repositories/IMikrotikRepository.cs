using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using System.Net.WebSockets;

namespace MTWireGuard.Application.Repositories
{
    public interface IMikrotikRepository : IDisposable
    {
        Task<List<LogViewModel>> GetLogsAsync();
        Task<List<WGServerViewModel>> GetServersAsync();
        Task<WGServerViewModel> GetServer(string Name);
        Task<List<ServerTrafficViewModel>> GetServersTraffic();
        Task<WGServerStatistics> GetServersCount();
        Task<List<WGPeerViewModel>> GetUsersAsync();
        Task<WGPeerViewModel> GetUser(int id);
        Task<string> GetUserHandshake(string id);
        Task<List<WGPeerLastHandshakeViewModel>> GetUsersHandshakes();
        Task<WGUserStatistics> GetUsersCount();
        Task<string> GetUserTunnelConfig(int id);
        Task<string> GetQRCodeBase64(int id);
        Task<string> GetUserV2rayQRCodeBase64(int id);
        Task<MTInfoViewModel> GetInfo();
        Task<IdentityViewModel> GetName();
        Task<CreationResult> SetName(IdentityUpdateModel identity);
        Task<bool> TryConnectAsync();
        Task<List<ActiveUserViewModel>> GetActiveSessions();
        Task<List<JobViewModel>> GetJobs();
        Task<string> GetCurrentSessionID();
        Task<string> KillJob(string JobID);
        Task<List<ScriptViewModel>> GetScripts();
        Task<CreationResult> CreateScript(Models.Mikrotik.ScriptCreateModel script);
        Task<string> RunScript(string name);
        Task<List<SchedulerViewModel>> GetSchedulers();
        Task<SchedulerViewModel> GetScheduler(int id);
        Task<CreationResult> CreateScheduler(Models.Mikrotik.SchedulerCreateModel scheduler);
        Task<CreationResult> UpdateScheduler(Models.Mikrotik.SchedulerUpdateModel scheduler);
        Task<CreationResult> CreateServer(ServerCreateModel server);
        Task<CreationResult> CreateUser(UserCreateModel peer);
        Task<CreationResult> UpdateUser(UserUpdateModel user);
        Task<CreationResult> UpdateServer(ServerUpdateModel server);
        Task<CreationResult> EnableServer(int id);
        Task<CreationResult> DisableServer(int id);
        Task<CreationResult> EnableUser(int id);
        Task<CreationResult> DisableUser(int id);
        Task<CreationResult> DeleteServer(int id);
        Task<CreationResult> DeleteUser(int id);
        Task<MikrotikAPI.Models.DNS> GetDNS();
        Task<CreationResult> SetDNS(DNSUpdateModel dns);
        Task<List<IPPoolViewModel>> GetIPPools();
        Task<CreationResult> CreateIPPool(PoolCreateModel ipPool);
        Task<CreationResult> UpdateIPPool(PoolUpdateModel ipPool);
        Task<CreationResult> DeleteIPPool(int id);
        Task<List<IPAddressViewModel>> GetIPAddresses();
        Task<List<IPAddressViewModel>> GetServerIP(string Name);
        Task<CreationResult> DeleteIPAddress(int id);
        Task<CreationResult> ResetUserTraffic(int id);
        Task<CreationResult> ImportUsers(List<UserImportModel> users, WebSocket socket);
        Task<CreationResult> ImportServers(List<ServerImportModel> servers, WebSocket socket);
    }
}
