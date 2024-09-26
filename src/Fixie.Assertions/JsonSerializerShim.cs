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
            writer.WriteRawValue("null");
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
            case byte v: writer.WriteRawValue(v.ToString()); return;
            case sbyte v: writer.WriteRawValue(v.ToString()); return;
            case short v: writer.WriteRawValue(v.ToString()); return;
            case ushort v: writer.WriteRawValue(v.ToString()); return;
            case int v: writer.WriteRawValue(v.ToString()); return;
            case uint v: writer.WriteRawValue(v.ToString()); return;
            case long v: writer.WriteRawValue(v.ToString()); return;
            case ulong v: writer.WriteRawValue(v.ToString()); return;
            case decimal v: writer.WriteRawValue(v.ToString()); return;
            case double v: writer.WriteRawValue(v.ToString()); return;
            case float v: writer.WriteRawValue(v.ToString()); return;
            case nint v: writer.WriteRawValue(v.ToString()); return;
            case nuint v: writer.WriteRawValue(v.ToString()); return;

            case bool v: writer.WriteRawValue(Serialize(v)); return;
            case char v: writer.WriteRawValue(Serialize(v)); return;
            case string v: writer.WriteRawValue(Serialize(v)); return;
            case Guid v: writer.WriteRawValue(Serialize(v)); return;
            case Type v: writer.WriteRawValue(Serialize(v)); return;
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
}