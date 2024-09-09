using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using Serilog;

namespace MTWireGuard.Application.Mapper
{
    public class PeerMapping : Profile
    {
        private readonly IServiceProvider _provider;
        private ILogger _logger;
        private IMemoryCache _memoryCache;
        private DBContext _db;
        private Dictionary<int, WGPeerDBModel> _userCache;
        private Dictionary<string, WGPeerLastHandshake> _handshakeCache;
        private Dictionary<int, SchedulerViewModel> _schedulerCache;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;

        public PeerMapping(IServiceProvider provider)
        {
            _provider = provider;
            Init();
            /*
             * Mikrotik Peer to ViewModel
            */
            CreateMap<WGPeer, WGPeerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)))
                .ForMember(dest => dest.AllowedAddress,
                    opt => opt.MapFrom(src => src.AllowedAddress))
                .ForMember(dest => dest.AllowedIPs,
                    opt => opt.MapFrom(src => GetPeerAllowedIPs(src)))
                .ForMember(dest => dest.CurrentAddress,
                    opt => opt.MapFrom(src => $"{src.CurrentEndpointAddress}:{src.CurrentEndpointPort}"))
                .ForMember(dest => dest.IsEnabled,
                    opt => opt.MapFrom(src => !src.Disabled))
                .ForMember(dest => dest.IsDifferent,
                    opt => opt.MapFrom(src => HasDifferences(src)))
                .ForMember(dest => dest.Upload,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TX), 2)))
                .ForMember(dest => dest.Download,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.RX), 2)))
                .ForMember(dest => dest.UploadBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TX)))
                .ForMember(dest => dest.DownloadBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.RX)))
                .ForMember(dest => dest.Expire,
                    opt => opt.MapFrom(src => ExpireDateToString(src)))
                .ForMember(dest => dest.LastHandshake,
                    opt => opt.MapFrom(src => GetPeerLastHandshake(src)))
                .ForMember(dest => dest.IPAddress,
                    opt => opt.MapFrom(src => src.ClientAddress))
                .ForMember(dest => dest.InheritDNS,
                    opt => opt.MapFrom(src => GetPeerInheritDNS(src)))
                .ForMember(dest => dest.InheritIP,
                    opt => opt.MapFrom(src => GetPeerInheritIP(src)))
                .ForMember(dest => dest.Traffic,
                    opt => opt.MapFrom(src => GetPeerTraffic(src)))
                .ForMember(dest => dest.TrafficUsed,
                    opt => opt.MapFrom(src => GetPeerTrafficUsage(src)));

            // WGPeer
            CreateMap<UserCreateModel, WGPeerCreateModel>()
                .ForMember(dest => dest.ClientAddress,
                    opt => opt.MapFrom(src => src.IPAddress));
            CreateMap<WGPeerCreateModel, WGPeerDBModel>();
            CreateMap<UserUpdateModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)))
                .ForMember(dest => dest.ClientAddress,
                    opt => opt.MapFrom(src => src.IPAddress));

            // DBUser
            CreateMap<WGPeerViewModel, WGPeerDBModel>();
            CreateMap<UserSyncModel, WGPeerDBModel>();
            CreateMap<UserSyncModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)));
            CreateMap<UserUpdateModel, WGPeerDBModel>();
        }

        private void Init()
        {
            _db = Provider.GetService<DBContext>();
            _logger = Provider.GetService<ILogger>();
            _memoryCache = Provider.GetService<IMemoryCache>();
            UpdateCache();
        }

        private void UpdateCache()
        {
            _userCache = _memoryCache.GetOrCreate(
                "DBUsers",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                    return _db.Users.ToDictionary(u => u.Id);
                });
        }

        private WGPeerDBModel GetDBUser(WGPeer source)
        {
            int id = Helper.ParseEntityID(source.Id);
            UpdateCache();
            _userCache.TryGetValue(id, out var user);
            return user;
        }

        private string GetPeerExpire(WGPeer source)
        {
            try
            {
                var dbuser = GetDBUser(source);
                if (dbuser == null || dbuser.ExpireID == 0) return string.Empty;
                var api = Provider.GetService<IMikrotikRepository>();
                _schedulerCache = _memoryCache.GetOrCreate(
                    "Schedulers",
                    cacheEntry =>
                    {
                        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                        return api.GetSchedulers().Result.ToDictionary(s => s.Id);
                    });
                _schedulerCache.TryGetValue((int)dbuser.ExpireID, out var expire);
                return expire != null ? expire.StartDate.ToDateTime(expire.StartTime).ToString("yyyy/MM/dd HH:mm:ss") : string.Empty;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Mapping Expire");
                return string.Empty;
            }
        }

        private string GetPeerLastHandshake(WGPeer source)
        {
            try
            {
                var api = Provider.GetService<IMikrotikRepository>();
                _handshakeCache = _memoryCache.GetOrCreate(
                    "Handshakes",
                    cacheEntry =>
                    {
                        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10);
                        return api.GetUsersHandshakes().Result.ToDictionary(u => u.Id);
                    });
                var handshake = _handshakeCache.TryGetValue(source.Id, out var lastHandshake);
                return lastHandshake?.LastHandshake ?? "Unknown";
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Handle Peer LastHandshake New");
                return "Unknown";
            }
        }

        private bool GetPeerInheritDNS(WGPeer source)
        {
            var user = GetDBUser(source);
            return user?.InheritDNS ?? false;
        }

        private bool GetPeerInheritIP(WGPeer source)
        {
            return (GetDBUser(source) != null) && GetDBUser(source).InheritIP;
        }

        private ulong GetPeerTrafficUsage(WGPeer source)
        {
            var dbItem = GetDBUser(source);
            return dbItem != null ? dbItem.RX + dbItem.TX : 0;
        }

        private int GetPeerTraffic(WGPeer source)
        {
            return (GetDBUser(source) != null) ? GetDBUser(source).TrafficLimit : 0;
        }

        private string GetPeerAllowedIPs(WGPeer source)
        {
            return (GetDBUser(source) != null) ? GetDBUser(source).AllowedIPs : "0.0.0.0/0";
        }

        private string ExpireDateToString(WGPeer source)
        {
            var expireDate = GetPeerExpire(source);
            return !string.IsNullOrWhiteSpace(expireDate) ? expireDate : "Unlimited";
        }

        private bool HasDifferences(WGPeer source)
        {
            var id = Helper.ParseEntityID(source.Id);
            var dbUser = GetDBUser(source);
            var dbTraffic = _db.LastKnownTraffic.Where(x => x.UserID == id).FirstOrDefault();
            return dbUser == null || dbTraffic == null || source.PrivateKey.Length < 5;
        }
    }
}
