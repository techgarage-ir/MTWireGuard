using Hangfire;
using Hangfire.Storage;
using MTWireGuard.Application.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTWireGuard.Application.Services
{
    public class HangfireManager
    {
        public static int SetUserExpiration(int userID, DateTime expireDate)
        {
            return int.Parse(BackgroundJob.Schedule<IMikrotikRepository>(mikrotik => mikrotik.DisableUser(userID), expireDate));
        }
    }
}
