using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MTWireGuard.Models;
using MTWireGuard.Models.Mikrotik;
using System.Text.RegularExpressions;

namespace MTWireGuard.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Logs
            CreateMap<Log, LogViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToUInt64(src.Id.Substring(1), 16)))
                .ForMember(dest =>dest.Topics,
                    opt => opt.MapFrom(src => FormatTopics(src.Topics)));

            // Server Traffic
            CreateMap<ServerTraffic, ServerTrafficViewModel>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Type,
                    opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Upload,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TX), 2)))
                .ForMember(dest => dest.Download,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.RX), 2)))
                .ForMember(dest => dest.UploadBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TX)))
                .ForMember(dest => dest.DownloadBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.RX)));

            // Mikrotik HWInfo
            CreateMap<MTInfo, MTInfoViewModel>()
                .ForMember(dest => dest.Architecture,
                    opt => opt.MapFrom(src => src.ArchitectureName))
                .ForMember(dest => dest.CPUCount,
                    opt => opt.MapFrom(src => Convert.ToByte(src.CPUCount)))
                .ForMember(dest => dest.CPUFrequency,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.CPUFrequency)))
                .ForMember(dest => dest.CPULoad,
                    opt => opt.MapFrom(src => Convert.ToByte(src.CPULoad)))
                .ForMember(dest => dest.TotalHDDBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TotalHDDSpace)))
                .ForMember(dest => dest.FreeHDDBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.FreeHDDSpace)))
                .ForMember(dest => dest.UsedHDDBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TotalHDDSpace) - Convert.ToInt64(src.FreeHDDSpace)))
                .ForMember(dest => dest.FreeHDDPercentage,
                    opt => opt.MapFrom(src => (byte)(Convert.ToInt64(src.FreeHDDSpace) * 100 / Convert.ToInt64(src.TotalHDDSpace))))
                .ForMember(dest => dest.TotalRAMBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TotalMemory)))
                .ForMember(dest => dest.FreeRAMBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.FreeMemory)))
                .ForMember(dest => dest.UsedRAMBytes,
                    opt => opt.MapFrom(src => Convert.ToInt64(src.TotalMemory) - Convert.ToInt64(src.FreeMemory)))
                .ForMember(dest => dest.FreeRAMPercentage,
                    opt => opt.MapFrom(src => (byte)(Convert.ToInt64(src.FreeMemory) * 100 / Convert.ToInt64(src.TotalMemory))))
                .ForMember(dest => dest.TotalHDD,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TotalHDDSpace), 2)))
                .ForMember(dest => dest.FreeHDD,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.FreeHDDSpace), 2)))
                .ForMember(dest => dest.UsedHDD,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TotalHDDSpace) - Convert.ToInt64(src.FreeHDDSpace), 2)))
                .ForMember(dest => dest.TotalRAM,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TotalMemory), 2)))
                .ForMember(dest => dest.FreeRAM,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.FreeMemory), 2)))
                .ForMember(dest => dest.UsedRAM,
                    opt => opt.MapFrom(src => Helper.ConvertByteSize(Convert.ToInt64(src.TotalMemory) - Convert.ToInt64(src.FreeMemory), 2)))
                .ForMember(dest => dest.UPTime,
                    opt => opt.MapFrom(src => FormatUptime(src.Uptime)));

            // Router Identity
            CreateMap<MTIdentity, MTIdentityViewModel>();

            // Active Users
            CreateMap<ActiveUser, ActiveUserViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.LoggedIn,
                    opt => opt.MapFrom(src => src.When));

            // Active Jobs
            CreateMap<Job, JobViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.NextId,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.NextId.Substring(1), 16)))
                .ForMember(dest => dest.Policies,
                    opt => opt.MapFrom(src => src.Policy.Split(',', StringSplitOptions.None).ToList()));

            // Item Creation
            CreateMap<CreationStatus, CreationResult>()
                .ForMember(dest => dest.Code,
                    opt => opt.MapFrom(src => (src.Success) ? "200" : src.Code.ToString()))
                .ForMember(dest => dest.Title,
                    opt => opt.MapFrom(src => (src.Success) ? "Done" : src.Message))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src => (src.Success) ? "Item created/updated successfully." : src.Detail));
        }

        private static List<string> FormatTopics(string topics)
        {
            return topics.Split(',', StringSplitOptions.TrimEntries).Select(t => t = Helper.UpperCaseTopics.Contains(t) ? t.ToUpper() : t.FirstCharToUpper()).ToList();
        }

        private static string FormatUptime(string uptime)
        {
            string patternWeek = "(\\d+)w",
                patternDay = "(\\d+)d",
                patternHour = "(\\d+)h",
                patternMinute = "(\\d+)m",
                patternSecond = "(\\d+)s";
            Regex weekRx = new(patternWeek),
                dayRx = new(patternDay),
                hourRx = new(patternHour),
                minuteRx = new(patternMinute),
                secondRx = new(patternSecond);
            string week = weekRx.Match(uptime).Value.RemoveNonNumerics(),
                day = dayRx.Match(uptime).Value.RemoveNonNumerics(),
                hour, minute, second;
            var hourMatch = hourRx.Match(uptime);
            var minuteMatch = minuteRx.Match(uptime);
            var secondMatch = secondRx.Match(uptime);
            hour = !string.IsNullOrWhiteSpace(hourMatch.Value.RemoveNonNumerics()) ? hourMatch.Value.RemoveNonNumerics() : "00";
            minute = !string.IsNullOrWhiteSpace(minuteMatch.Value.RemoveNonNumerics()) ? minuteMatch.Value.RemoveNonNumerics() : "00";
            second = !string.IsNullOrWhiteSpace(secondMatch.Value.RemoveNonNumerics()) ? secondMatch.Value.RemoveNonNumerics() : "00";
            hour = int.Parse(hour.RemoveNonNumerics()).ToString("D2");
            minute = int.Parse(minute.RemoveNonNumerics()).ToString("D2");
            second = int.Parse(second.RemoveNonNumerics()).ToString("D2");
            return $"{week}w {day}d {hour}:{minute}:{second}";
        }
    }
}
