using System.Runtime.ExceptionServices;
using System.Text;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    public static string Serialize<TValue>(TValue value, CsonSerializerOptions options)
    {
        var output = new StringBuilder();
        var writer = new CsonWriter(output);

        SerializeInternal(writer, value, options);

        return output.ToString();
    }

    public static void SerializeInternal<TValue>(CsonWriter writer, TValue value, CsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteRawValue("null");
            return;
        }

        var type = typeof(TValue);
        
        if (type == typeof(object))
            type = value.GetType();
        
        var converter = options.Converters.First(x => x.CanConvert(type));

        if (converter.GetType().IsSubclassOf(typeof(CsonConverterFactory)))
            converter = ((CsonConverterFactory)converter).CreateConverter(type);

        var write = converter.GetType().GetMethod("Write")!;
        try
        {
            write.Invoke(converter, [writer, value, options]);
        }
        catch (Exception exception)
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
            T value,
#nullable restore
            CsonSerializerOptions options);
}

abstract class CsonConverter
{
    public abstract bool CanConvert(Type typeToConvert);
}

abstract class CsonConverterFactory : CsonConverter
{
    public abstract CsonConverter CreateConverter(Type typeToConvert);
}

class CsonSerializerOptions
{
    public List<CsonConverter> Converters { get; } = [];
}

class CsonException : Exception
{
    public CsonException(string message)
        : base(message)
    {
    }
}