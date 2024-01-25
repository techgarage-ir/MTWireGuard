using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Models.Mikrotik
{
    public class SchedulerViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public DateOnly StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan Interval { get; set; }
        public List<string> Policies { get; set; }
        public int RunCount { get; set; }
        public DateTime NextRun { get; set; }
        public string OnEvent { get; set; }
        public bool Enabled { get; set; }
    }
    public class SchedulerCreateModel
    {
        public string Name { get; set; }
        public DateOnly? StartDate { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? Interval { get; set; }
        public List<string>? Policies { get; set; }
        public string? OnEvent { get; set; }
    }
}
