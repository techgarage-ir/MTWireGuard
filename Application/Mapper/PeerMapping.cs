using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models.Mikrotik;

namespace MTWireGuard.Application.Mapper
{
    public class PeerMapping : Profile
    {
        private readonly IServiceProvider _provider;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;

        public PeerMapping(IServiceProvider provider)
        {
            _provider = provider;
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
                    opt => opt.MapFrom(src => Convert.ToInt64(src.RX)));

            // WGPeer
            CreateMap<UserCreateModel, WGPeerCreateModel>();
            CreateMap<WGPeerCreateModel, WGPeerDBModel>();
            CreateMap<UserUpdateModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));

            // DBUser
            CreateMap<WGPeerViewModel, WGPeerDBModel>();
            CreateMap<UserSyncModel, WGPeerDBModel>();
            CreateMap<UserSyncModel, WGPeerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));
            CreateMap<UserUpdateModel, WGPeerDBModel>();
        }

        private string? GetPeerName(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).Name : "";
        }

        private string? GetPeerPrivateKey(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            return (db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)) != null) ? db.Users.ToList().Find(u => u.Id == Convert.ToInt32(source.Id[1..], 16)).PrivateKey : "";
        }

        private bool HasDifferences(WGPeer source)
        {
            var db = Provider.GetService<DBContext>();
            var id = Convert.ToInt32(source.Id[1..], 16);
            var dbUser = db.Users.ToList().Find(x => x.Id == id);
            if (dbUser is null) return true;
            if (dbUser.PublicKey != source.PublicKey) return true;
            return string.IsNullOrWhiteSpace(dbUser.PrivateKey);
        }
    }
}
