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
        if (HasDictionaryRepresentation(type, out var keyType, out var valueType))
            return GetDynamicWriter(nameof(Writer.WriteDictionary), keyType, valueType);

        if (HasListRepresentation(type, out var itemType))
            return GetDynamicWriter(nameof(Writer.WriteList), itemType);

        if (type.IsEnum)
            return GetDynamicWriter(nameof(Writer.WriteEnum), type);

        return GetDynamicWriter(nameof(Writer.WriteProperties), type);
    }

    static bool HasDictionaryRepresentation(Type typeToConvert,
        [NotNullWhen(true)] out Type? keyType,
        [NotNullWhen(true)] out Type? valueType)
    {
        var interfaceType =
            IsDictionaryInterface(typeToConvert)
                ? typeToConvert
                : typeToConvert.GetInterfaces().FirstOrDefault(IsDictionaryInterface);

        if (interfaceType == null)
        {
            keyType = null;
            valueType = null;
            
            return false;
        }

        var typeArguments = interfaceType.GetGenericArguments();
        
        keyType = typeArguments[0];
        valueType = typeArguments[1];
        
        return true;
    }

    static bool HasListRepresentation(Type typeToConvert, [NotNullWhen(true)] out Type? itemType)
    {
        var interfaceType =
            IsEnumerableInterface(typeToConvert)
                ? typeToConvert
                : typeToConvert.GetInterfaces().FirstOrDefault(IsEnumerableInterface);

        if (interfaceType == null)
        {
            itemType = null;

            return false;
        }

        itemType = interfaceType.GetGenericArguments()[0];

        return true;
    }

    static bool IsEnumerableInterface(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

    static bool IsDictionaryInterface(Type type)
    {
        if (!type.IsGenericType)
            return false;

        var definition = type.GetGenericTypeDefinition();

        return definition == typeof(IDictionary<,>)
            || definition == typeof(IReadOnlyDictionary<,>);
    }

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