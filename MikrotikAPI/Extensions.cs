using Newtonsoft.Json;

namespace MikrotikAPI
{
    public static class Extensions
    {
        public static T ToModel<T>(this string str)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(str)) return default;
                return JsonConvert.DeserializeObject<T>(str);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return default;
            }
        }
    }
}
