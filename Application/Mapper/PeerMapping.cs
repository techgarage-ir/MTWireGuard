using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Application.Mapper
{
    public class PeerMapping : Profile
    {
        private readonly IServiceProvider _provider;
        //private readonly IMemoryCache _cache;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;

        public PeerMapping(IServiceProvider provider/*, IMemoryCache memoryCache*/)
        {
            _provider = provider;
            //_cache = memoryCache;
            /*
             * Mikrotik Peer to ViewModel
            */
            CreateMap<WGPeer, WGPeerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt32(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.AllowedAddress,
                    opt => opt.MapFrom(src => src.AllowedAddress))
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
        private async Task<string> GetPeerExpire(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            var api = Provider.GetService<IMikrotikRepository>();
            var schedulers = await api.GetSchedulers();
            var dbuser = db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id));
            if (dbuser == null || dbuser.ExpireID == 0) return string.Empty;
            var expire = schedulers.Find(s => s.Id == dbuser.ExpireID); // User parser for sched id
            return expire != null ? expire.StartDate.ToDateTime(expire.StartTime).ToString("yyyy/MM/dd HH:mm:ss") : string.Empty;
        }

        private string GetPeerLastHandshake(WGPeer source)
        {
            try
            {
                var api = Provider.GetService<IMikrotikRepository>();
                var lastHandshake = api.GetUserHandshake(source.Id).Result;
                return lastHandshake;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Unknown";
            }
        }

        private bool GetPeerInheritDNS(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)) != null) && db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)).InheritDNS;
        }

        private bool GetPeerInheritIP(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)) != null) && db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)).InheritIP;
        }

        private ulong GetPeerTrafficUsage(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            var dbItem = db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id));
            return dbItem != null ? dbItem.RX + dbItem.TX : 0;
        }

        private int GetPeerTraffic(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)) != null) ? db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id)).TrafficLimit : 0;
        }

        private string ExpireDateToString(WGPeer source)
        {
            var expireDate = GetPeerExpire(source).Result;
            return !string.IsNullOrWhiteSpace(expireDate) ? expireDate : "Unlimited";
        }

        private DateTime ExpireStringToDate(string expire)
        {
            return expire == "Unlimited" ? new() : Convert.ToDateTime(expire);
        }

        private bool HasDifferences(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            var id = Helper.ParseEntityID(source.Id);
            var dbUser = db.Users.ToList().Find(x => x.Id == id);
            var dbTraffic = db.LastKnownTraffic.ToList().Find(x => x.UserID == id);
            return dbUser == null || dbTraffic == null || source.PrivateKey.Length < 5;
        }
    }
}
