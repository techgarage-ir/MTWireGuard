using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using MikrotikAPI.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Utils;

namespace MTWireGuard.Application.Mapper
{
    public class ServerMapping : Profile
    {
        private readonly IServiceProvider _provider;
        private IMemoryCache _memoryCache;
        private DBContext _db;
        private Dictionary<int, WGServerDBModel> _serverCache;
        private Dictionary<int, IPPoolViewModel> _ipPoolCache;
        private DNS _dnsCache;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;
        public ServerMapping(IServiceProvider provider)
        {
            _provider = provider;
            Init();
            /*
             * Convert Mikrotik Server Model to ViewModel
            */
            CreateMap<WGServer, WGServerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt32(ConverterUtil.ParseEntityID(src.Id))))
                .ForMember(dest => dest.IsEnabled,
                    opt => opt.MapFrom(src => !src.Disabled))
                .ForMember(dest => dest.IPAddress,
                    opt => opt.MapFrom(src => GetServerIP(src).Result))
                .ForMember(dest => dest.InheritDNS,
                    opt => opt.MapFrom(src => GetServerInheritDNS(src)))
                .ForMember(dest => dest.DNSAddress,
                    opt => opt.MapFrom(src => GetServerDNS(src)))
                .ForMember(dest => dest.UseIPPool,
                    opt => opt.MapFrom(src => GetServerUseIPPool(src)))
                .ForMember(dest => dest.IPPool,
                    opt => opt.MapFrom(src => GetServerIPPool(src)));

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
                    opt => opt.MapFrom(src => ConverterUtil.ParseEntityID(src.Id)));
        }

        private void Init()
        {
            _db = Provider.GetService<DBContext>();
            _memoryCache = Provider.GetService<IMemoryCache>();
        }

        private void UpdateCache()
        {
            _serverCache = _memoryCache.GetOrCreate(
                "DBServers",
                cacheEntry =>
                {
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                    return _db.Servers.ToDictionary(s => s.Id);
                });
            _ipPoolCache = _memoryCache.GetOrCreate(
                "IPPools",
                cacheEntry =>
                {
                    var api = Provider.GetService<IMikrotikRepository>();
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                    return api.GetIPPools().Result.ToDictionary(i => i.Id);
                });
            _dnsCache = _memoryCache.GetOrCreate(
                "DNS",
                cacheEntry =>
                {
                    var api = Provider.GetService<IMikrotikRepository>();
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                    return api.GetDNS().Result;
                });
        }

        private WGServerDBModel GetDBServer(WGServer source)
        {
            int id = ConverterUtil.ParseEntityID(source.Id);
            UpdateCache();
            _serverCache.TryGetValue(id, out var server);
            return server;
        }

        private async Task<string> GetServerIP(WGServer source)
        {
            var api = Provider.GetService<IMikrotikRepository>();
            var ifIP = await api.GetServerIP(source.Name);
            return ifIP.Count != 0 ? ifIP.FirstOrDefault().Address : "0.0.0.0";
        }

        private bool GetServerInheritDNS(WGServer source)
        {
            var dbItem = GetDBServer(source);
            return dbItem != null && dbItem.InheritDNS;
        }

        private string GetServerDNS(WGServer source)
        {
            var dbItem = GetDBServer(source);
            var mtDNS = _dnsCache.Servers;
            return dbItem == null || dbItem.InheritDNS ? mtDNS : dbItem.DNSAddress;
        }

        private bool GetServerUseIPPool(WGServer source)
        {
            var dbItem = GetDBServer(source);
            return dbItem != null && dbItem.UseIPPool;
        }

        private string GetServerIPPool(WGServer source)
        {
            var dbItem = GetDBServer(source);
            if (dbItem == null || dbItem.IPPoolId == null)
                return string.Empty;
            _ipPoolCache.TryGetValue((int)dbItem.IPPoolId, out var ipPool);
            return ipPool != null ? ipPool.Ranges.FirstOrDefault() : string.Empty;
        }
    }
}
