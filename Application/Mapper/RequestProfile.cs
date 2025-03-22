using AutoMapper;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Models.Responses;
using MTWireGuard.Application.Models.Requests;
using MTWireGuard.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MikrotikAPI.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using MTWireGuard.Application.Repositories;
using MTWireGuard.Application.Utils;

namespace MTWireGuard.Application.Mapper
{
    public class RequestProfile : Profile
    {
        private readonly IServiceProvider _provider;
        private ILogger _logger;
        private IMemoryCache _memoryCache;
        private DBContext _db;
        private Dictionary<string, IPPoolViewModel> _poolCache;
        private IServiceProvider Provider => _provider.CreateScope().ServiceProvider;
        public RequestProfile(IServiceProvider provider)
        {
            _provider = provider;
            Init();
            // Peer Request
            CreateMap<CreateClientRequest, UserCreateModel>()
                .ForMember(dest => dest.Disabled,
                    opt => opt.MapFrom(src => !src.Enabled))
                .ForMember(dest => dest.EndpointAddress,
                    opt => opt.MapFrom(src => src.Endpoint))
                .ForMember(dest => dest.PersistentKeepalive,
                    opt => opt.MapFrom(src => src.KeepAlive.ToString()))
                .ForMember(dest => dest.Expire,
                    opt => opt.MapFrom(src => (!string.IsNullOrWhiteSpace(src.Expire)) ? Convert.ToDateTime(src.Expire) : (DateTime?)null));

            CreateMap<UpdateClientRequest, UserUpdateModel>()
                .ForMember(dest => dest.EndpointAddress,
                    opt => opt.MapFrom(src => src.Endpoint))
                .ForMember(dest => dest.PersistentKeepalive,
                    opt => opt.MapFrom(src => src.KeepAlive))
                .ForMember(dest => dest.Expire,
                    opt => opt.MapFrom(src => Convert.ToDateTime(src.Expire)));

            CreateMap<ImportUsersItem, UserImportModel>()
                .ForMember(dest => dest.Enabled,
                    opt => opt.MapFrom(src => src.IsEnabled));

            // Server Request
            CreateMap<CreateServerRequest, ServerCreateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

            CreateMap<UpdateServerRequest, ServerUpdateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

            CreateMap<ImportServersItem, ServerImportModel>()
                .ForMember(dest => dest.Enabled,
                    opt => opt.MapFrom(src => src.IsEnabled))
                .ForMember(dest => dest.IPPoolId,
                    opt => opt.MapFrom(src => GetIPPoolId(src)));

            // IP Pool Request
            CreateMap<CreatePoolRequest, PoolCreateModel>()
                .ForMember(dest => dest.Ranges,
                    opt => opt.MapFrom(src => string.Join(',', src.Ranges)));
            CreateMap<UpdateIPPoolRequest, PoolUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"))
                .ForMember(dest => dest.Ranges,
                    opt => opt.MapFrom(src => string.Join(',', src.Ranges)));
            // IP Pool Create Model
            CreateMap<PoolCreateModel, MikrotikAPI.Models.IPPoolCreateModel>()
                .ForMember(dest => dest.NextPool,
                    opt => opt.MapFrom(src => src.Next));
            // IP Pool Update Model
            CreateMap<PoolUpdateModel, MikrotikAPI.Models.IPPoolUpdateModel>()
                .ForMember(dest => dest.NextPool,
                    opt => opt.MapFrom(src => src.Next));

            // Item Creation
            CreateMap<MikrotikAPI.Models.CreationStatus, CreationResult>()
                .ForMember(dest => dest.Code,
                    opt => opt.MapFrom(src => (src.Success) ? "200" : src.Code.ToString()))
                .ForMember(dest => dest.Title,
                    opt => opt.MapFrom(src => (src.Success) ? "Done" : src.Message))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => (src.Success) ? "Item created/updated successfully." : src.Detail));

            // Toast Result
            CreateMap<CreationResult, ToastMessage>()
                .ForMember(dest => dest.Title,
                    opt => opt.MapFrom(src => src.Code == "200" ? src.Title : $"[{src.Code}] {src.Title}"))
                .ForMember(dest => dest.Body,
                    opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Background,
                    opt => opt.MapFrom(src => src.Code == "200" ? "success" : "danger"));
        }

        private void Init()
        {
            _logger = Provider.GetService<ILogger>();
            _memoryCache = Provider.GetService<IMemoryCache>();
        }

        private int GetIPPoolId(ImportServersItem source)
        {
            try
            {
                if (source == null || string.IsNullOrWhiteSpace(source.IPPool))
                {
                    return 0;
                }
                _poolCache = _memoryCache.GetOrCreate(
                    "IPPools",
                    cacheEntry =>
                    {
                        cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(3);
                        var api = Provider.GetService<IMikrotikRepository>();
                        return api.GetIPPools().Result.ToDictionary(p => p.Ranges.FirstOrDefault() ?? string.Empty);
                    });
                if (_poolCache.TryGetValue(source.IPPool, out var ipPool))
                    return ipPool != null ? ipPool.Id : 0;
                else
                    return 0;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Mapping IPPool");
                return 0;
            }
        }
    }
}
