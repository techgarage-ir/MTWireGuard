using Newtonsoft.Json.Schema;
using System.Runtime.InteropServices;

namespace MTWireGuard.Application.Utils
{
    public static class Constants
    {
        public static readonly string[] SizeSuffixes = ["bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"];

        public static readonly string[] UpperCaseTopics =
        [
            "dhcp",
            "ppp",
            "l2tp",
            "pptp",
            "sstp"
        ];

        public static string PeersTrafficUsageScript(string apiURL)
        {
            return $"/tool fetch mode=http url=\"{apiURL}\" http-method=post check-certificate=no output=none http-data=([/queue/simple/print stats proplist=name,bytes as-value]);";
        }

        public static string UserExpirationScript(string userID)
        {
            return $"/interface/wireguard/peers/disable {userID}";
        }

        private static JSchema iPApiSchema;

        public static JSchema IPApiSchema
        {
            get
            {
                return iPApiSchema;
            }
            set
            {
                if (iPApiSchema == null)
                {
                    iPApiSchema = value;
                }
                else
                {
                    throw new InvalidOperationException("IPApiSchema can only be set once.");
                }
            }
        }

        public static string DataPath()
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Join("/home", "app") : Path.Join(AppDomain.CurrentDomain.BaseDirectory);
        }

        public static string DataPath(string filename)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Path.Join("/home", "app", filename) : Path.Join(AppDomain.CurrentDomain.BaseDirectory, filename);
        }
    }
}
