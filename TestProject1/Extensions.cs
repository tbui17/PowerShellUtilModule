using System.Dynamic;
using System.Linq.Expressions;
using Autofac;
using Autofac.Builder;
using AutoMapper;
using Newtonsoft.Json;
using PowerShellStandardModule1.Models;

namespace TestProject1;

internal static class Extensions
{
    public static TReturn Thru<T, TReturn>(this T src, Func<T, TReturn> fn) => fn(src);
    public static void Tap<T>(this T src, Action<T> fn) => fn(src);

    public static T Clone<T>(this T src) =>
        JsonConvert
           .SerializeObject(src)
           .Thru(JsonConvert.DeserializeObject<T>)!;

    public static string Serialize<T>(this T src, Formatting formatting = Formatting.None)
    {
        return JsonConvert.SerializeObject(
            src, new JsonSerializerSettings
            {
                Formatting = formatting,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = [new DirectoryInfoConverter()]
            }
        );
    }

    public static void Log(this object src) => Console.WriteLine(src);

    public static object Get(this ExpandoObject src, string key)
    {
        var dict = (IDictionary<string, object>)src!;
        return dict[key];
    }

    /// <summary>
    /// Sets mapping from source property to destination property. Convenient extension method. 
    /// </summary>
    public static IMappingExpression<TSource, TDestination> MapProperty<TSource, TDestination, TProperty>(
        this IMappingExpression<TSource, TDestination> map,
        Expression<Func<TSource, TProperty>> sourceMember,
        Expression<Func<TDestination, object>> targetMember
    )
    {
        map.ForMember(targetMember, opt => opt.MapFrom(sourceMember));

        return map;
    }

    public static IEnumerable<int> Times(this int count) => Enumerable.Range(0, count);

    public static IRegistrationBuilder<T, ConcreteReflectionActivatorData, SingleRegistrationStyle> RegisterNamed<T>(this ContainerBuilder b, string name) where T : notnull
    {
        return b.RegisterType<T>().Named<T>(name);
    }
}

public class DirectoryInfoConverter : JsonConverter<DirectoryInfo>
{
    public override void WriteJson(JsonWriter writer, DirectoryInfo? value, JsonSerializer serializer)
    {
        writer.WriteValue(value!.FullName);
    }

    public override DirectoryInfo ReadJson(
        JsonReader reader,
        Type objectType,
        DirectoryInfo? existingValue,
        bool hasExistingValue,
        JsonSerializer serializer
    )
    {
        var path = (string)reader.Value!;
        return new DirectoryInfo(path);
    }
}



public static class TreeNode
{
    public static TreeNode<TValue> ToSerializable<T, TValue>(this TreeNode<T> node, Func<T, TValue> selector) =>
        new()
        {
            Value = selector(node.Value),
            Height = node.Height,
            Index = node.Index,
            Children = node
               .Children.Select(x => x.ToSerializable(selector))
               .ToList()
        };

    public static TreeNode<T> ToSerializable<T>(this TreeNode<T> node)
    {
        var clone = node.Clone();
        clone.Parent = null;
        clone.Children = node
           .Children.Select(x => x.ToSerializable())
           .ToList();
        return clone;
    }


    public static TreeNode<T> RestoreParents<T>(this TreeNode<T> node)
    {
        foreach (var child in node.Children)
        {
            child.Parent = node;
            child.RestoreParents();
        }

        return node;
    }

    public static TreeNode<T>? JsonParse<T>(string json) =>
        JsonConvert
           .DeserializeObject<TreeNode<T>>(json)
          ?.RestoreParents();
}