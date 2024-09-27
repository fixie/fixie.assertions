using System.Diagnostics;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static MethodInfo SerializeInternalDefinition = typeof(CsonSerializer).GetMethod("SerializeInternal")!;

    public static string Serialize<TValue>(TValue value)
    {
        var output = new StringBuilder();
        var writer = new CsonWriter(output);

        SerializeInternal(writer, value);

        return output.ToString();
    }

    public static void SerializeInternal<TValue>(CsonWriter writer, TValue value)
    {
        if (value is null)
        {
            writer.Write("null");
            return;
        }

        var underlyingType = Nullable.GetUnderlyingType(typeof(TValue));

        if (underlyingType != null)
        {
            SerializeViaReflection(underlyingType, writer, value);
            return;
        }

        switch (value)
        {
            case byte v: writer.Write(v.ToString()); return;
            case sbyte v: writer.Write(v.ToString()); return;
            case short v: writer.Write(v.ToString()); return;
            case ushort v: writer.Write(v.ToString()); return;
            case int v: writer.Write(v.ToString()); return;
            case uint v: writer.Write(v.ToString()); return;
            case long v: writer.Write(v.ToString()); return;
            case ulong v: writer.Write(v.ToString()); return;
            case decimal v: writer.Write(v.ToString()); return;
            case double v: writer.Write(v.ToString()); return;
            case float v: writer.Write(v.ToString()); return;
            case nint v: writer.Write(v.ToString()); return;
            case nuint v: writer.Write(v.ToString()); return;

            case bool v: writer.Write(Serialize(v)); return;
            case char v: writer.Write(Serialize(v)); return;
            case string v: writer.Write(Serialize(v)); return;
            case Guid v: writer.Write(Serialize(v)); return;
            case Type v: writer.Write(Serialize(v)); return;
        };

        var type = typeof(TValue);
        
        if (type == typeof(object))
            type = value.GetType();

        var converter = GetDynamicConverter(type);

        WriteViaReflection(converter, writer, value);
    }

    static MethodInfo GetDynamicConverter(Type type)
    {
        var pairType = GetPairType(type);
        if (pairType != null)
        {
            var typeArguments = pairType.GetGenericArguments();
            var keyType = typeArguments[0];
            var valueType = typeArguments[1];

            return GetDynamicConverter("WritePairsLiteral", keyType, valueType);
        }

        var enumerableType = GetEnumerableType(type);
        if (enumerableType != null)
        {
            var itemType = enumerableType.GetGenericArguments()[0];

            return GetDynamicConverter("WriteListLiteral", itemType);
        }

        return GetDynamicConverter(
            type.IsEnum
                ? "WriteEnumLiteral"
                : "WritePropertiesLiteral", type);
    }

    static MethodInfo GetDynamicConverter(string method, params Type[] typeArguments)
    {
        return typeof(CsonSerializer)
            .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic)?
            .MakeGenericMethod(typeArguments)
            ?? throw new UnreachableException();
    }

    static void WriteViaReflection<TValue>(MethodInfo converter, CsonWriter writer, TValue value)
    {
        try
        {
            converter.Invoke(null, [writer, value]);
        }
        catch (TargetInvocationException exception)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
            throw; // Unreachable.
        }
    }

    static void SerializeViaReflection(Type type, CsonWriter writer, object value)
    {
        try
        {
            SerializeInternalDefinition
                .MakeGenericMethod(type)
                .Invoke(null, [writer, value]);
        }
        catch (TargetInvocationException  exception)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
            throw; // Unreachable.
        }
    }

    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);

        if (underlyingType != null)
            return $"typeof({TypeName(underlyingType)}?)";

        return $"typeof({TypeName(type)})";
    }

    static string TypeName(Type x)
        => x switch
            {
                _ when x == typeof(bool) => "bool",
                _ when x == typeof(sbyte) => "sbyte",
                _ when x == typeof(byte) => "byte",
                _ when x == typeof(short) => "short",
                _ when x == typeof(ushort) => "ushort",
                _ when x == typeof(int) => "int",
                _ when x == typeof(uint) => "uint",
                _ when x == typeof(long) => "long",
                _ when x == typeof(ulong) => "ulong",
                _ when x == typeof(nint) => "nint",
                _ when x == typeof(nuint) => "nuint",
                _ when x == typeof(decimal) => "decimal",
                _ when x == typeof(double) => "double",
                _ when x == typeof(float) => "float",
                _ when x == typeof(char) => "char",
                _ when x == typeof(string) => "string",
                _ when x == typeof(object) => "object",
                _ => x.ToString()
            };
}