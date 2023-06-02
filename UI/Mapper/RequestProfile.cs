using AutoMapper;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Models.Requests;
using MTWireGuard.Models.Responses;

namespace MTWireGuard.Mapper
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
                    opt => opt.MapFrom(src => src.KeepAlive.ToString()));

            CreateMap<SyncUserRequest, UserSyncModel>();

            CreateMap<UpdateClientRequest, UserUpdateModel>()
                .ForMember(dest => dest.EndpointAddress,
                    opt => opt.MapFrom(src => src.Endpoint))
                .ForMember(dest => dest.PersistentKeepalive,
                    opt => opt.MapFrom(src => src.KeepAlive));

            // Server Request
            CreateMap<CreateServerRequest, ServerCreateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

            CreateMap<UpdateServerRequest, ServerUpdateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

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
