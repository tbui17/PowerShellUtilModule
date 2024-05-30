using System.Dynamic;
using Newtonsoft.Json;

namespace TestProject1;

internal static class Extensions
{
   

    public static TReturn Thru<T, TReturn>(this T src, Func<T, TReturn> fn) => fn(src);
    public static void Tap<T>(this T src, Action<T> fn) => fn(src);

    public static T Clone<T>(this T src) => JsonConvert.SerializeObject(src).Thru(JsonConvert.DeserializeObject<T>)!;
    
    public static string Serialize<T>(this T src, Formatting formatting = Formatting.None) => JsonConvert.SerializeObject(src,formatting);
    
    public static void Log(this object src) => Console.WriteLine(src);

    public static object Get(this ExpandoObject src, string key)
    {
        var dict = (IDictionary<string, object>)src!;
        return dict[key];
    }
    
        
    
}