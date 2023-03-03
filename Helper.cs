using MTWireGuard.Models.Mikrotik;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.Compression;
using System.Text;

namespace MTWireGuard
{
    public class Helper
    {
        public static readonly string[] UpperCaseTopics =
        {
                "dhcp",
                "ppp",
                "l2tp",
                "pptp",
                "sstp"
            };

        private static readonly string[] SizeSuffixes =
                   { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
        public static string ConvertByteSize(long value, int decimalPlaces = 2)
        {
            if (decimalPlaces < 0) { throw new ArgumentOutOfRangeException("decimalPlaces"); }
            if (value < 0) { return "-" + ConvertByteSize(-value, decimalPlaces); }
            if (value == 0) { return "0"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                SizeSuffixes[mag]);
        }
    }

    public static class StringCompression
    {
        public static byte[] Compress(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);

            using MemoryStream msi = new(bytes);
            using MemoryStream mso = new();
            using (GZipStream gs = new(mso, CompressionMode.Compress))
            {
                msi.CopyTo(gs);
            }

            return mso.ToArray();
        }

        public static string Decompress(byte[] zip)
        {
            using MemoryStream msi = new(zip);
            using MemoryStream mso = new();
            using (GZipStream gs = new(msi, CompressionMode.Decompress))
            {
                gs.CopyTo(mso);
            }

            return Encoding.UTF8.GetString(mso.ToArray());
        }
    }

    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
    }
}
