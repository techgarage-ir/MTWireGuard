using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace MTWireGuard.Application.Utils
{
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

    public static partial class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };

        public static string RemoveNonNumerics(this string input) => Numerics().Replace(input, "");
        [GeneratedRegex("[^0-9.]")]
        private static partial Regex Numerics();
    }
}
