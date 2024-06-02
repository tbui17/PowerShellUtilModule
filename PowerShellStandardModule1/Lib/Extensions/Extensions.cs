using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using AutoMapper;
using PowerShellStandardModule1.Attributes;
using PowerShellStandardModule1.Models;

namespace PowerShellStandardModule1.Lib.Extensions;

public static class Extensions
{
    public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            queue.Enqueue(item);
        }
    }

    public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            stack.Push(item);
        }
    }

    public static void ForEach<T>(this IEnumerable<T> items, Action<T> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static void ForEach<T, TReturn>(this IEnumerable<T> items, Func<T, TReturn> action)
    {
        foreach (var item in items)
        {
            action(item);
        }
    }

    public static Stack<T> ToStack<T>(this IEnumerable<T> items) => new(items);

    public static bool NotEmpty<T>(this Stack<T> stack) => stack.Count != 0;

    public static TReturn Pipe<T, TReturn>(this T src, Func<T, TReturn> fn) => fn(src);

    public static T Tap<T>(this T src, Action<T> fn)
    {
        fn(src);
        return src;
    }

    public static T Tap<T, TReturn>(this T src, Func<T, TReturn> fn)
    {
        fn(src);
        return src;
    }

    public static (IList<T> True, IList<T> False) PartitionBy<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        var left = new List<T>();
        var right = new List<T>();

        foreach (var item in source)
        {
            if (predicate(item))
            {
                left.Add(item);
            }
            else
            {
                right.Add(item);
            }
        }

        return (left, right);
    }

    public static Func<T, TReturn> Compose<T, TIntermediate, TReturn>(
        this Func<T, TIntermediate> fn1,
        Func<TIntermediate, TReturn> fn2
    ) =>
        x => fn2(fn1(x));


    public static string StringJoin<T>(this IEnumerable<T> items, string separator) => string.Join(separator, items);

    public static StringBuilder ToStringBuilder(this IEnumerable<string> strings) =>
        strings.Aggregate(new StringBuilder(), (sb, x) => sb.AppendLine(x));

    public static void SetFieldValue<T>(this T src, string key, object value) where T : notnull
    {
        var prop = src
               .GetType()
               .GetField(key) ??
            throw new ArgumentException($"Field {key} not found on {src.GetType().Name}");
        prop.SetValue(src, value);
    }

    public static object? GetFieldValue<T>(this T src, string key) where T : notnull
    {
        var prop = src
               .GetType()
               .GetField(key) ??
            throw new ArgumentException($"Field {key} not found on {src.GetType().Name}");
        return prop.GetValue(src);
    }

    public static void SetPropertyValue<T>(this T src, string key, object value) where T : notnull
    {
        var prop = src
               .GetType()
               .GetProperty(key) ??
            throw new ArgumentException($"Property {key} not found on {src.GetType().Name}");
        prop.SetValue(src, value);
    }

    public static object? GetPropertyValue<T>(this T src, string key) where T : notnull
    {
        var prop = src
               .GetType()
               .GetProperty(key) ??
            throw new ArgumentException($"Property {key} not found on {src.GetType().Name}");
        return prop.GetValue(src);
    }

    public static IEnumerable<KeyValuePair<TKey, TReturn>> SelectValues<TKey, TValue, TReturn>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        Func<TValue, TReturn> fn
    ) =>
        source.Select(x => new KeyValuePair<TKey, TReturn>(x.Key, fn(x.Value)));


    public static IEnumerable<KeyValuePair<TKey, TReturn>> SelectValues<TKey, TValue, TReturn>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        Func<TValue, TKey, TReturn> fn
    ) =>
        source.Select(x => new KeyValuePair<TKey, TReturn>(x.Key, fn(x.Value, x.Key)));


    public static IEnumerable<KeyValuePair<T, TReturn>> SelectManyToPairs<T, TReturn>(
        this IEnumerable<T> source,
        Func<T, IEnumerable<TReturn>> fn
    )
    {
        return from item in source
            from innerItem in fn(item)
            select KeyValuePair.Create(item, innerItem);
    }


    public static IEnumerable<KeyValuePair<TKey, TReturn>> ChooseEntries<TKey, TValue, TReturn>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        Func<TValue, IEnumerable<TReturn>> fn
    ) =>
        source.SelectMany(
            x => fn(x.Value)
               .Select(y => new KeyValuePair<TKey, TReturn>(x.Key, y))
        );


    public static IEnumerable<KeyValuePair<TKey, TReturn>> ToPairs<TKey, TReturn>(
        this IEnumerable<TKey> source,
        Func<TKey, TReturn> fn
    ) =>
        source.Select(x => new KeyValuePair<TKey, TReturn>(x, fn(x)));

    public static IEnumerable<string> GetFieldKeys<T>(this T src) where T : notnull =>
        src
           .GetType()
           .GetFields()
           .Select(x => x.Name);

    public static IEnumerable<string> GetPropertyKeys<T>(this T src) where T : notnull =>
        src
           .GetType()
           .GetProperties()
           .Select(x => x.Name);

    public static IEnumerable<Action> ToActions<T>(this IEnumerable<T> items, Action<T> action) =>
        items.Select(x => new Action(() => action(x)));

    public static IEnumerable<KeyValuePair<string, object?>> ToPairsFromFields(this object obj)
    {
        return obj
           .GetFieldKeys()
           .ToPairs(obj.GetFieldValue);
    }

    public static IEnumerable<KeyValuePair<string, object?>> ToPairsFromProperties(this object obj)
    {
        return obj
           .GetPropertyKeys()
           .ToPairs(obj.GetPropertyValue);
    }

    public static IEnumerable<KeyValuePair<string, T>> ToPairsFromProperties<T>(this object obj)
    {
        foreach (var item in obj.ToPairsFromProperties())
        {
            if (item.Value is T val)
            {
                yield return item.WithValue(val);
            }
        }
    }


    public static IEnumerable<KeyValuePair<TKey, TValue>>?
        CastKeyValuePairs<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, object>> source)
    {
        return source as IEnumerable<KeyValuePair<TKey, TValue>>;
    }

    public static IEnumerable<TKey> Keys<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) =>
        source.Select(x => x.Key);

    public static IEnumerable<TValue> Values<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source) =>
        source.Select(x => x.Value);

    public static IEnumerable<KeyValuePair<TKey, TResult>> JoinToKeyValuePairs<TOuter, TInner, TKey, TResult>(
        this IEnumerable<TOuter> outerItems,
        IEnumerable<TInner> innerItems,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector,
        Func<TOuter, TInner, TResult> resultSelector
    ) where TKey : notnull
    {
        var outer = outerItems.ToImmutableDictionary(outerKeySelector);
        var inner = innerItems.ToImmutableDictionary(innerKeySelector);
        var commonKeys = outer.Keys.Intersect(inner.Keys);

        return commonKeys.Select(
            key =>
            {
                var res = resultSelector(outer[key], inner[key]);
                var kvp = KeyValuePair.Create(key, res);
                return kvp;
            }
        );
    }

    public static IEnumerable<KeyValuePair<TKey, (TOuter, TInner)>> Join<TOuter, TInner, TKey>(
        this IEnumerable<TOuter> outerItems,
        IEnumerable<TInner> innerItems,
        Func<TOuter, TKey> outerKeySelector,
        Func<TInner, TKey> innerKeySelector
    ) where TKey : notnull
    {
        return outerItems.JoinToKeyValuePairs(
            innerItems, outerKeySelector, innerKeySelector,
            (x, y) => (x, y)
        );
    }

    public static IEnumerable<KeyValuePair<TKey, (TValue, TValue2)>> Join<TKey, TValue, TValue2>(
        this IEnumerable<KeyValuePair<TKey, TValue>> outerItems,
        IEnumerable<KeyValuePair<TKey, TValue2>> innerItems
    ) where TKey : notnull
    {
        var outer = outerItems.ToImmutableDictionary();
        var inner = innerItems.ToImmutableDictionary();
        var commonKeys = outer.Keys.Intersect(inner.Keys);

        return commonKeys.Select(
            key =>
            {
                var res = (outer[key], inner[key]);
                var kvp = KeyValuePair.Create(key, res);
                return kvp;
            }
        );
    }


    public static IEnumerable<KeyValuePair<string, T>> JoinObject<T>(
        this object obj1,
        object obj2,
        Func<object?, object?, T> joiner
    )
    {
        var keys1 = obj1
           .GetFieldKeys()
           .ToHashSet();
        var keys2 = obj2
           .GetFieldKeys()
           .ToHashSet();

        var commonKeys = keys1.Intersect(keys2);

        foreach (var key in commonKeys)
        {
            var val1 = obj1.GetFieldValue(key);
            var val2 = obj2.GetFieldValue(key);

            var res = joiner(val1, val2);
            var kvp = new KeyValuePair<string, T>(key, res);
            yield return kvp;
        }
    }

    public static KeyValuePair<TKey, TResult> WithValue<TKey, TValue, TResult>(
        this KeyValuePair<TKey, TValue> source,
        TResult value
    )
    {
        return KeyValuePair.Create(source.Key, value);
    }

    public static T[] InArray<T>(this T item) => [item];

    public static IEnumerable<(T Item1, TReturn Item2)> ToTuples<T, TReturn>(
        this IEnumerable<T> items,
        Func<T, TReturn> fn
    ) =>
        items.Select(x => (x, fn(x)));


    public static IEnumerable<(T Item1, TReturn Item2)> SelectManyToTuples<T, TReturn>(
        this IEnumerable<T> items,
        Func<T, IEnumerable<TReturn>> fn
    )
    {
        return from item in items
            from innerItem in fn(item)
            select (item, innerItem);
    }


    public static IEnumerable<KeyValuePair<PropertyInfo, T>> GetPropertiesWithAttribute<T>(this object obj)
        where T : Attribute
    {
        return obj
           .GetType()
           .GetProperties()
           .SelectManyToPairs(
                x => x.GetCustomAttribute<T>(false) is { } attr
                    ? attr.InArray()
                    : []
            );
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

    public static IEnumerable<(T, T)> Sliding<T>(this IEnumerable<T> src)
    {
        using var iterator = src.GetEnumerator();


        if (!iterator.MoveNext()) yield break;
        
        var prev = iterator.Current;

        while (iterator.MoveNext())
        {
            yield return (prev, iterator.Current);
            prev = iterator.Current;
        }
        
    }
}

public static class Stack
{
    public static Stack<T> From<T>(IEnumerable<T> items) => new(items);
}