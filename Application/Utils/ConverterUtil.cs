namespace MTWireGuard.Application.Utils
{
    public class ConverterUtil
    {
        #region Mikrotik Entity
        public static int ParseEntityID(string entityID)
        {
            return Convert.ToInt32(entityID[1..], 16);
        }

        public static string ParseEntityID(int entityID)
        {
            return $"*{entityID:X}";
        }

        public static TimeSpan ConvertToTimeSpan(string input)
        {
            int weeks = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;
            int seconds = 0;

            if (input.Contains('w'))
            {
                string w = input.Split('w').First();
                weeks = int.Parse(w);
                input = input.Remove(0, input.IndexOf('w') + 1);
            }
            if (input.Contains('d'))
            {
                string d = input.Split('d').First();
                days = int.Parse(d);
                input = input.Remove(0, input.IndexOf('d') + 1);
            }
            if (input.Contains('h'))
            {
                string h = input.Split('h').First();
                hours = int.Parse(h);
                input = input.Remove(0, input.IndexOf('h') + 1);
            }
            if (input.Contains('m'))
            {
                string m = input.Split('m').First();
                minutes = int.Parse(m);
                input = input.Remove(0, input.IndexOf('m') + 1);
            }
            if (input.Contains('s'))
            {
                string s = input.Split('s').First();
                seconds = int.Parse(s);
            }

            return new TimeSpan((weeks * 7) + days, hours, minutes, seconds);
        }
        #endregion

        #region Byte Converter
        public static string ConvertByteSize(long value, int decimalPlaces = 2)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
            ArgumentOutOfRangeException.ThrowIfNegative(value);
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
                Constants.SizeSuffixes[mag]);
        }
        public static string ConvertByteSize(ulong value, int decimalPlaces = 2)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(decimalPlaces);
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == 0) { return "0"; }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1UL << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                mag += 1;
                adjustedSize /= 1024;
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                Constants.SizeSuffixes[mag]);
        }
        public static string ConvertQueueByteSize(ulong value)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            if (value == 0) { return "0"; }

            double size = value;
            int unitIndex = 0;

            while (size >= 1000 && unitIndex < Constants.SizeSuffixes.Length - 1)
            {
                size /= 1000;
                unitIndex++;
            }

            return $"{size:0.##} {Constants.SizeSuffixes[unitIndex]}";
        }

        public static ulong GigabyteToByte(int gigabyte)
        {
            return Convert.ToUInt64(gigabyte * (1024L * 1024 * 1024));
        }

        public static (ulong Upload, ulong Download) ParseBytesTouple(string input)
        {
            var parts = input.Split('/');
            if (parts.Length != 2 || !ulong.TryParse(parts[0], out var upload) || !ulong.TryParse(parts[1], out var download))
            {
                throw new ArgumentException("Invalid format. Expected format: 'upload/download'", nameof(input));
            }

            return (upload, download);
        }
        #endregion
    }
}
