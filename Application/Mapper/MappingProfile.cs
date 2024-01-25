using AutoMapper;
using MTWireGuard.Application.MinimalAPI;
using MTWireGuard.Application.Models;
using MTWireGuard.Application.Models.Mikrotik;
using MTWireGuard.Application.Models.Requests;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MTWireGuard.Application.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Logs
            CreateMap<MikrotikAPI.Models.Log, LogViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToUInt64(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.Topics,
                    opt => opt.MapFrom(src => FormatTopics(src.Topics)));

            // Server Traffic
            CreateMap<MikrotikAPI.Models.ServerTraffic, ServerTrafficViewModel>()
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
            CreateMap<MikrotikAPI.Models.MTInfo, MTInfoViewModel>()
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
            CreateMap<MikrotikAPI.Models.MTIdentity, IdentityViewModel>();
            CreateMap<IdentityUpdateModel, MikrotikAPI.Models.MTIdentityUpdateModel>();
            CreateMap<UpdateIdentityRequest, IdentityUpdateModel>();

            // Router DNS
            CreateMap<DNSUpdateModel, MikrotikAPI.Models.MTDNSUpdateModel>()
                .ForMember(dest => dest.Servers,
                    opt => opt.MapFrom(src => string.Join(',', src.Servers)));
            CreateMap<UpdateDNSRequest, DNSUpdateModel>();

            // Active Users
            CreateMap<MikrotikAPI.Models.ActiveUser, ActiveUserViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.LoggedIn,
                    opt => opt.MapFrom(src => src.When));

            // Active Jobs
            CreateMap<MikrotikAPI.Models.Job, JobViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.NextId,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.NextId.Substring(1), 16)))
                .ForMember(dest => dest.Policies,
                    opt => opt.MapFrom(src => src.Policy.Split(',', StringSplitOptions.None).ToList()));

            // Scripts
            CreateMap<MikrotikAPI.Models.Script, ScriptViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.IsValid,
                    opt => opt.MapFrom(src => !src.Invalid))
                .ForMember(dest => dest.LastStarted,
                    // opt => opt.MapFrom(src => DateTime.ParseExact(src.LastStarted, "MMM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture)))
                    opt => opt.MapFrom(src => ConvertToDateTime(src.LastStarted)))
                .ForMember(dest => dest.Policies,
                    opt => opt.MapFrom(src => src.Policy.Split(',', StringSplitOptions.None).ToList()));
            CreateMap<ScriptCreateModel, MikrotikAPI.Models.ScriptCreateModel>()
                .ForMember(dest => dest.Policy,
                    opt => opt.MapFrom(src => string.Join(',', src.Policies)));

            // Schedulers
            CreateMap<MikrotikAPI.Models.Scheduler, SchedulerViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Convert.ToInt16(src.Id.Substring(1), 16)))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => ConvertToDate(src.StartDate)))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src => (src.StartTime != "startup") ? TimeSpan.ParseExact(src.StartTime, "hh\\:mm\\:ss", CultureInfo.InvariantCulture) : new TimeSpan()))
                .ForMember(dest => dest.Interval,
                    opt => opt.MapFrom(src => ConvertToTimeSpan(src.Interval)))
                .ForMember(dest => dest.NextRun,
                    opt => opt.MapFrom(src => ConvertToDateTime(src.NextRun)))
                .ForMember(dest => dest.Policies,
                    opt => opt.MapFrom(src => src.Policy.Split(',', StringSplitOptions.None).ToList()))
                .ForMember(dest => dest.Enabled,
                    opt => opt.MapFrom(src => !src.Disabled));
            CreateMap<SchedulerCreateModel, MikrotikAPI.Models.SchedulerCreateModel>()
                .ForMember(dest => dest.Policy,
                    opt => opt.MapFrom(src => string.Join(',', src.Policies)))
                .ForMember(dest => dest.StartDate,
                    opt => opt.MapFrom(src => DateToString(src.StartDate)))
                .ForMember(dest => dest.StartTime,
                    opt => opt.MapFrom(src => TimeToString(src.StartTime)))
                .ForMember(dest => dest.Interval,
                    opt => opt.MapFrom(src => TimeToString(src.Interval)));

            // IPAddress
            CreateMap<MikrotikAPI.Models.IPAddress, IPAddressViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)))
                .ForMember(dest => dest.Enabled,
                    opt => opt.MapFrom(src => !src.Disabled))
                .ForMember(dest => dest.Valid,
                    opt => opt.MapFrom(src => !src.Invalid));

            // IP Pools
            CreateMap<MikrotikAPI.Models.IPPool, IPPoolViewModel>()
                .ForMember(dest => dest.Id,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)))
                .ForMember(dest => dest.Ranges,
                    opt => opt.MapFrom(src => src.Ranges.Split(',', StringSplitOptions.None).ToList()));

            // Data Usages
            CreateMap<UsageObject, DataUsage>()
                .ForMember(dest => dest.Id,
                    opt => opt.Ignore())
                .ForMember(dest => dest.UserID,
                    opt => opt.MapFrom(src => Helper.ParseEntityID(src.Id)))
                .ForMember(dest => dest.CreationTime,
                    opt => opt.MapFrom(src => DateTime.Now))
                .ForMember(dest => dest.RX,
                    opt => opt.MapFrom(src => src.RX))
                .ForMember(dest => dest.TX,
                    opt => opt.MapFrom(src => src.TX));
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

        private static TimeSpan FormatTime(string timeString)
        {
            TimeSpan timeSpan = TimeSpan.Zero;
            if (TimeSpan.TryParseExact(timeString, @"h\hm\ms\s", null, out timeSpan))
            {
                return timeSpan;
            }
            else
            {
                throw new Exception("Invalid TimeSpan format");
            }
        }

        private static TimeSpan ConvertToTimeSpan(string input)
        {
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            string[] parts = input.Split('h', 'm', 's');

            foreach (string part in parts)
            {
                if (string.IsNullOrEmpty(part))
                    continue;

                if (int.TryParse(part, out int value))
                {
                    if (input.Contains('h'))
                    {
                        hours = value;
                    }
                    else if (input.Contains('m'))
                    {
                        minutes = value;
                    }
                    else if (input.Contains('s'))
                    {
                        seconds = value;
                    }
                }
            }

            return new TimeSpan(hours, minutes, seconds);
        }

        private static DateTime ConvertToDateTime(string input)
        {
            //input ??= "00:00:00";
            string inputs = input == null || input == "" ? "00:00:00" : input;
            string[] formats = ["yyyy-MM-dd HH:mm:ss", "MMM/dd HH:mm:ss", "HH:mm:ss"];
            try
            {
                return DateTime.ParseExact(inputs, formats, CultureInfo.InvariantCulture);
            }
            catch
            {
                throw;
            }
        }

        private static DateOnly ConvertToDate(string input)
        {
            return DateOnly.ParseExact(input, ["MMM/dd/yyyy", "yyyy-MM-dd"]);
        }

        private static string DateToString(DateOnly? date)
        {
            return date.HasValue ? date.Value.ToString("yyyy-MM-dd") : string.Empty;
        }

        private static string TimeToString(TimeSpan? time)
        {
            return time.HasValue ? time.Value.ToString(@"hh\:mm\:ss") : string.Empty;
        }
    }
}
