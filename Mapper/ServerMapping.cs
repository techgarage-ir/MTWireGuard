using AutoMapper;
using MTWireGuard.Models.Mikrotik;
using MTWireGuard.Models.Requests;

namespace MTWireGuard.Mapper
{
    public class ServerMapping : Profile
    {
        public ServerMapping()
        {
            /*
             * Convert Mikrotik Server Model to ViewModel
            */
            CreateMap<WGServer, WGServerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt32(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.IsEnabled,
                    opt => opt.MapFrom(src => !src.Disabled));

            /*
             * Convert Wrapper CreateModel to Rest-API CreateModel
            */
            CreateMap<CreateServerRequest, ServerCreateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));

            CreateMap<ServerCreateModel, WGServerCreateModel>()
                .ForMember(dest => dest.Disabled,
                    opt => opt.MapFrom(src => !src.Enabled));

            /*
             * Convert Wrapper UpdateModel to Rest-API UpdateModel
            */
            CreateMap<UpdateServerRequest, ServerUpdateModel>()
                .ForMember(dest => dest.ListenPort,
                    opt => opt.MapFrom(src => src.Port));


            CreateMap<ServerUpdateModel, WGServerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));
        }
    }
}
