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

namespace MTWireGuard.Application.Mapper
{
    public class RequestProfile : Profile
    {
        public RequestProfile()
        {
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

            CreateMap<SyncUserRequest, UserSyncModel>();

            CreateMap<UpdateClientRequest, UserUpdateModel>()
                .ForMember(dest => dest.EndpointAddress,
                    opt => opt.MapFrom(src => src.Endpoint))
                .ForMember(dest => dest.PersistentKeepalive,
                    opt => opt.MapFrom(src => src.KeepAlive))
                .ForMember(dest => dest.Expire,
                    opt => opt.MapFrom(src => Convert.ToDateTime(src.Expire)));

            // Server Request
            CreateMap<CreateServerRequest, ServerCreateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

            CreateMap<UpdateServerRequest, ServerUpdateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

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
    }
}
