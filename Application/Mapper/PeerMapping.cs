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
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => GetPeerName(src)))
                .ForMember(dest => dest.PrivateKey,
                    opt => opt.MapFrom(src => GetPeerPrivateKey(src)))
                .ForMember(dest => dest.Address,
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
                .ForMember(dest => dest.DNSAddress,
                    opt => opt.MapFrom(src => GetPeerDNS(src)))
                .ForMember(dest => dest.InheritDNS,
                    opt => opt.MapFrom(src => GetPeerInheritDNS(src)))
                .ForMember(dest => dest.InheritIP,
                    opt => opt.MapFrom(src => GetPeerInheritIP(src)))
                .ForMember(dest => dest.Traffic,
                    opt => opt.MapFrom(src => GetPeerTraffic(src)))
                .ForMember(dest => dest.TrafficUsed,
                    opt => opt.MapFrom(src => GetPeerTrafficUsage(src)));

            // WGPeer
            CreateMap<UserCreateModel, WGPeerCreateModel>();
            CreateMap<WGPeerCreateModel, WGPeerDBModel>();
            CreateMap<UserUpdateModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));

            // DBUser
            CreateMap<WGPeerViewModel, WGPeerDBModel>()
                .ForMember(dest => dest.Expire,
                    opt => opt.MapFrom(src => ExpireStringToDate(src.Expire)));
            CreateMap<UserSyncModel, WGPeerDBModel>();
            CreateMap<UserSyncModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));
            CreateMap<UserUpdateModel, WGPeerDBModel>();
        }

        private string GetPeerName(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).Name ?? string.Empty : string.Empty;
        }

        private string? GetPeerPrivateKey(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).PrivateKey ?? string.Empty : string.Empty;
        }

        private DateTime GetPeerExpire(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).Expire ?? new() : new();
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

        private string? GetPeerDNS(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).DNSAddress ?? string.Empty : string.Empty;
        }

        private bool GetPeerInheritDNS(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) && db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).InheritDNS;
        }

        private bool GetPeerInheritIP(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) && db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).InheritIP;
        }

        private int GetPeerTrafficUsage(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            //var dbItem = db.DataUsages.ToList().Find(u => u.UserID == Convert.ToInt32(source.Id[1..], 16));
            //if (source.AllowedAddress == "172.16.33.29/32")
            //    Console.WriteLine();
            //return (dbItem != null) ? dbItem.RX + dbItem.TX : 0;
            var dbItem = db.Users.ToList().Find(u => u.Id == Helper.ParseEntityID(source.Id));
            return dbItem != null ? dbItem.RX + dbItem.TX : 0;
        }

        private int GetPeerTraffic(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).TrafficLimit : 0;
        }

        private string ExpireDateToString(WGPeer source)
        {
            var expireDate = GetPeerExpire(source);
            return expireDate != new DateTime() ? expireDate.ToString() : "Unlimited";
        }

        private DateTime ExpireStringToDate(string expire)
        {
            return expire == "Unlimited" ? new() : Convert.ToDateTime(expire);
        }

        private bool HasDifferences(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            var id = Convert.ToInt32(source.Id[1..], 16);
            var dbUser = db.Users.ToList().Find(x => x.Id == id);
            var dbTraffic = db.LastKnownTraffic.ToList().Find(x => x.UserID == id);
            if (dbUser is null || dbTraffic is null) return true;
            if (dbUser.PublicKey != source.PublicKey) return true;
            return string.IsNullOrWhiteSpace(dbUser.PrivateKey);
        }
    }
}
