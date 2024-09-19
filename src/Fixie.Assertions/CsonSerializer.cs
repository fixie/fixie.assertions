using System.Text.Json;
using System.Text.Json.Serialization;

namespace Fixie.Assertions;

class CsonSerializer
{
    static readonly JsonSerializerOptions JsonSerializerOptions;

    static CsonSerializer()
    {
        JsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        JsonSerializerOptions.Converters.Add(new TypeLiteral());
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, JsonSerializerOptions);

    class TypeLiteral : JsonConverter<Type>
    {
        public override Type Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new NotImplementedException();

        public override void Write(Utf8JsonWriter writer, Type value, JsonSerializerOptions options)
            => writer.WriteRawValue(Serialize(value), skipInputValidation: true);
    }

    static string Serialize(Type x) =>
        $"typeof({x switch
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
        }})";
}