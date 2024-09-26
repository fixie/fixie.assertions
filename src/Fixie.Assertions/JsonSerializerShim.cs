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
        
        CsonConverterFactory[] factories = [
            new EnumLiteralFactory(),
            new PairsLiteralFactory(),
            new ListLiteralFactory(),
            new PropertiesLiteralFactory()
        ];
        var factory = factories.First(x => x.CanConvert(type));
        var converter = factory.CreateConverter(type);

        var write = converter.GetType().GetMethod("Write")!;
        try
        {
            write.Invoke(converter, [writer, value]);
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

abstract class CsonConverter<T> : CsonConverter
{
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert == typeof(T);

        public abstract void Write(
            CsonWriter writer,
#nullable disable // T may or may not be nullable depending on the derived converter's HandleNull override.
            T value
#nullable restore
            );
}

abstract class CsonConverter
{
    public abstract bool CanConvert(Type typeToConvert);
}

abstract class CsonConverterFactory : CsonConverter
{
    public abstract CsonConverter CreateConverter(Type typeToConvert);
}

class CsonException : Exception
{
    public CsonException(string message)
        : base(message)
    {
    }
}