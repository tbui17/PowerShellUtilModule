using Newtonsoft.Json;

namespace TestProject1;

internal static class Extensions
{
   

    public static TReturn Thru<T, TReturn>(this T src, Func<T, TReturn> fn) => fn(src);
    public static void Tap<T>(this T src, Action<T> fn) => fn(src);

    public static T Clone<T>(this T src) => JsonConvert.SerializeObject(src).Thru(JsonConvert.DeserializeObject<T>)!;
    
        
    
}