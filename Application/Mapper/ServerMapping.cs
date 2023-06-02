using AutoMapper;
using MTWireGuard.Application.Models.Mikrotik;

namespace MTWireGuard.Application.Mapper
{
    public class ServerMapping : Profile
    {
        public ServerMapping()
        {
            /*
             * Convert Mikrotik Server Model to ViewModel
            */
            CreateMap<MikrotikAPI.Models.WGServer, WGServerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt32(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.IsEnabled,
                    opt => opt.MapFrom(src => !src.Disabled));

            /*
             * Convert Wrapper CreateModel to Rest-API CreateModel
            */
            CreateMap<ServerCreateModel, MikrotikAPI.Models.WGServerCreateModel>()
                .ForMember(dest => dest.Disabled,
                    opt => opt.MapFrom(src => !src.Enabled));

            /*
             * Convert Wrapper UpdateModel to Rest-API UpdateModel
            */
            CreateMap<ServerUpdateModel, MikrotikAPI.Models.WGServerUpdateModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => $"*{src.Id:X}"));
        }
    }
}
