using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace Fixie.Assertions;

class Serializer
{
    static MethodInfo SerializeInternalDefinition = typeof(Serializer).GetMethod("SerializeInternal")!;

    public static string Serialize<TValue>(TValue value)
    {
        var output = new StringBuilder();
        var writer = new Writer(output);

        SerializeInternal(writer, value);

        return output.ToString();
    }

    public static void SerializeInternal<TValue>(Writer writer, TValue value)
    {
        if (value is null)
        {
            writer.WriteNull();
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
            case byte v: writer.WriteNumber(v); return;
            case sbyte v: writer.WriteNumber(v); return;
            case short v: writer.WriteNumber(v); return;
            case ushort v: writer.WriteNumber(v); return;
            case int v: writer.WriteNumber(v); return;
            case uint v: writer.WriteNumber(v); return;
            case long v: writer.WriteNumber(v); return;
            case ulong v: writer.WriteNumber(v); return;
            case decimal v: writer.WriteNumber(v); return;
            case double v: writer.WriteNumber(v); return;
            case float v: writer.WriteNumber(v); return;
            case nint v: writer.WriteNumber(v); return;
            case nuint v: writer.WriteNumber(v); return;

            case bool v: writer.WriteBool(v); return;
            case char v: writer.WriteChar(v); return;
            case string v: writer.WriteString(v); return;
            case Guid v: writer.WriteGuid(v); return;
            case Type v: writer.WriteType(v); return;
        };

        var type = typeof(TValue);
        
        if (type == typeof(object))
            type = value.GetType();

        var converter = GetDynamicConverter(type);

        WriteViaReflection(converter, writer, value);
    }

    static MethodInfo GetDynamicConverter(Type type)
    {
        if (IsPairType(type, out var keyType, out var valueType))
            return GetDynamicWriter(nameof(Writer.WritePairs), keyType, valueType);

        if (IsEnumerableType(type, out var itemType))
            return GetDynamicWriter(nameof(Writer.WriteList), itemType);

        return GetDynamicWriter(
            type.IsEnum
                ? nameof(Writer.WriteEnum)
                : nameof(Writer.WriteProperties), type);
    }

    static bool IsPairType(Type typeToConvert,
        [NotNullWhen(true)] out Type? keyType,
        [NotNullWhen(true)] out Type? valueType)
    {
        if (IsEnumerableType(typeToConvert, out var itemType))
        {
            if (itemType.IsGenericType &&
                itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                var typeArguments = itemType.GetGenericArguments();

                keyType = typeArguments[0];
                valueType = typeArguments[1];
                return true;
            }
        }

        keyType = null;
        valueType = null;
        return false;
    }

    static bool IsEnumerableType(Type typeToConvert, [NotNullWhen(true)] out Type? itemType)
    {
        var enumerableType =
            IsEnumerableT(typeToConvert)
                ? typeToConvert
                : typeToConvert.GetInterfaces().FirstOrDefault(IsEnumerableT);

        itemType = enumerableType?.GetGenericArguments()[0];

        return itemType != null;
    }

    static bool IsEnumerableT(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

    static MethodInfo GetDynamicWriter(string method, params Type[] typeArguments)
    {
        return typeof(Writer)
            .GetMethod(method, BindingFlags.Instance | BindingFlags.Public)?
            .MakeGenericMethod(typeArguments)
            ?? throw new UnreachableException();
    }

    static void WriteViaReflection<TValue>(MethodInfo converter, Writer writer, TValue value)
    {
        try
        {
            converter.Invoke(writer, [value]);
        }
        catch (TargetInvocationException exception)
        {
            ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
            throw; // Unreachable.
        }
    }

    static void SerializeViaReflection(Type type, Writer writer, object value)
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