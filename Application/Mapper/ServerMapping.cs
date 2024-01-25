using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;

namespace MTWireGuard.Application.Mapper
{
    public class ServerMapping : Profile
    {
        private readonly IServiceProvider _provider;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;
        public ServerMapping(IServiceProvider provider)
        {
            _provider = provider;
            /*
             * Convert Mikrotik Server Model to ViewModel
            */
            CreateMap<WGServer, WGServerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt32(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.IsEnabled,
                    opt => opt.MapFrom(src => !src.Disabled))
                .ForMember(dest => dest.IPAddress,
                    opt => opt.MapFrom(src => GetServerIP(src).Result))
                .ForMember(dest => dest.InheritDNS,
                    opt => opt.MapFrom(src => GetServerInheritDNS(src)))
                .ForMember(dest => dest.DNSAddress,
                    opt => opt.MapFrom(src => GetServerDNS(src).Result))
                .ForMember(dest => dest.UseIPPool,
                    opt => opt.MapFrom(src => GetServerUseIPPool(src)))
                .ForMember(dest => dest.IPPool,
                    opt => opt.MapFrom(src => GetServerIPPool(src).Result));

            /*
             * Convert Wrapper CreateModel to Rest-API CreateModel
            */
            CreateMap<ServerCreateModel, WGServerCreateModel>()
                .ForMember(dest => dest.Disabled,
                    opt => opt.MapFrom(src => !src.Enabled));

            /*
             * Convert Wrapper UpdateModel to Rest-API UpdateModel
            */
            CreateMap<ServerUpdateModel, WGServerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));
        }

        private async Task<string> GetServerIP(WGServer source)
        {
            var api = Provider.GetService<IMikrotikRepository>();
            var ifIP = await api.GetServerIP(source.Name);
            return ifIP.Any() ? ifIP.FirstOrDefault().Address : "0.0.0.0";
        }

        private bool GetServerInheritDNS(WGServer source)
        {
            var db = Provider.GetService<DBContext>();
            var api = Provider.GetService<IMikrotikRepository>();
            var dbItem = db.Servers.ToList().Find(s => s.Id == Helper.ParseEntityID(source.Id));
            return dbItem != null && dbItem.InheritDNS;
        }

        private async Task<string> GetServerDNS(WGServer source)
        {
            var db = Provider.GetService<DBContext>();
            var api = Provider.GetService<IMikrotikRepository>();
            var mtDNS = (await api.GetDNS()).Servers;
            var dbItem = db.Servers.ToList().Find(s => s.Id == Helper.ParseEntityID(source.Id));
            return dbItem == null || dbItem.InheritDNS ? mtDNS : dbItem.DNSAddress;
        }

        private bool GetServerUseIPPool(WGServer source)
        {
            var db = Provider.GetService<DBContext>();
            var api = Provider.GetService<IMikrotikRepository>();
            var dbItem = db.Servers.ToList().Find(s => s.Id == Helper.ParseEntityID(source.Id));
            return dbItem != null && dbItem.UseIPPool;
        }

        private async Task<string> GetServerIPPool(WGServer source)
        {
            var db = Provider.GetService<DBContext>();
            var api = Provider.GetService<IMikrotikRepository>();
            var dbItem = db.Servers.ToList().Find(s => s.Id == Helper.ParseEntityID(source.Id));
            if (dbItem == null || dbItem.IPPoolId == null)
                return string.Empty;
            var pools = await api.GetIPPools();
            return pools.Find(p => p.Id == dbItem.IPPoolId).Ranges.FirstOrDefault();
        }
    }
}
