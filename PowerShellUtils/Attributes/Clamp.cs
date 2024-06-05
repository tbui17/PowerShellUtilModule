using System;
using PowerShellStandardModule1.Lib.Extensions;

namespace PowerShellStandardModule1.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class Clamp : Attribute
{
    public int Min { get; init; } = int.MinValue;
    public int Max { get; init; } = int.MaxValue;

    public static void Invoke(object obj)
    {

        obj
           .GetPropertiesWithAttribute<Clamp>()
           .ForEach(
                x =>
                {
                    var (prop, attr) = x;

                    var val = (int)prop.GetValue(obj)!;
                    var newVal = int.Clamp(val, attr.Min, attr.Max);
                    prop.SetValue(obj, newVal);
                }
            );
    }
}