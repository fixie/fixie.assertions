using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static Fixie.Assertions.StringUtilities;
using static System.Environment;

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

        JsonSerializerOptions.Converters.Add(new RawStringLiteral<nint>());
        JsonSerializerOptions.Converters.Add(new RawStringLiteral<nuint>());

        JsonSerializerOptions.Converters.Add(new CharacterLiteral());
        JsonSerializerOptions.Converters.Add(new StringLiteral());
        JsonSerializerOptions.Converters.Add(new EnumLiteralFactory());
        JsonSerializerOptions.Converters.Add(new TypeLiteral());

        JsonSerializerOptions.Converters.Add(new ListLiteralFactory());
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, JsonSerializerOptions);

    abstract class CsonConverter<T> : JsonConverter<T>
    {
        public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new UnreachableException();

        public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
        {
            if (value != null)
                writer.WriteRawValue(RawValue(value), skipInputValidation: true);
        }

        protected abstract string RawValue(T value);
    }

    class RawStringLiteral<T> : CsonConverter<T> where T : struct
    {
        protected override string RawValue(T value)
            => value.ToString() ?? "null";
    }

    class CharacterLiteral : CsonConverter<char>
    {
        protected override string RawValue(char value)
            => Serialize(value);
    }

    class StringLiteral : CsonConverter<string>
    {
        protected override string RawValue(string value)
            => Serialize(value);
    }

    class EnumLiteral<T> : CsonConverter<T> where T : struct, Enum
    {
        protected override string RawValue(T value)
        {
            if (Enum.IsDefined(typeof(T), value))
                return $"{typeof(T).FullName}.{value}";
            
            var numeric = value.ToString();

            if (numeric.StartsWith('-'))
                return $"({typeof(T).FullName})({value})";
            else
                return $"({typeof(T).FullName}){value}";
        }
    }

    class EnumLiteralFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum;

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Type converterType = typeof(EnumLiteral<>).MakeGenericType(typeToConvert);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class TypeLiteral : CsonConverter<Type>
    {
        protected override string RawValue(Type value)
            => Serialize(value);
    }

    class ListLiteralFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            var enumerableType = GetEnumerableType(typeToConvert);
            
            if (enumerableType != null)
            {
                //TODO: Once we have a higher priority factory detecting
                //IEnumerable<KeyValuePair<TKey, TValue>>, this whole
                //method can become a simple check for `GetEnumerableType(...) != null`,
                //as this next condition becomes always-false.
                if (GetPairType(typeToConvert) != null)
                    return false;

                return true;
            }

            return false;
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            var enumerableType = GetEnumerableType(typeToConvert) ?? throw new UnreachableException();
            var itemType = enumerableType.GetGenericArguments()[0];

            Type converterType = typeof(ListLiteral<>).MakeGenericType(itemType);
            return (JsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class ListLiteral<T> : JsonConverter<IEnumerable<T>>
    {
        public override IEnumerable<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => throw new UnreachableException();

        public override void Write(Utf8JsonWriter writer, IEnumerable<T> value, JsonSerializerOptions options)
        {
            writer.WriteStartArray();

            foreach (var item in value)
                JsonSerializer.Serialize(writer, item, options);

            writer.WriteEndArray();
        }
    }

    static Type? GetEnumerableType(Type typeToConvert)
    {
        if (IsEnumerableT(typeToConvert))
            return typeToConvert;

        return typeToConvert.GetInterfaces().FirstOrDefault(IsEnumerableT);
    }

    static bool IsEnumerableT(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

    static string Serialize(char x) => $"'{Escape(x)}'";

    static string Serialize(string x)
    {
        if (IsMultiline(x))
        {
            var terminal = RawStringTerminal(x);

            return $"{terminal}{NewLine}{x}{NewLine}{terminal}";
        }
        
        return $"\"{string.Join("", x.Select(Escape))}\"";
    }

    static string RawStringTerminal(string x)
    {
        var longestDoubleQuoteSequence = 0;
        var currentDoubleQuoteSequence = 0;

        foreach (var c in x)
        {
            if (c != '\"')
            {
                currentDoubleQuoteSequence = 0;
                continue;
            }

            currentDoubleQuoteSequence++;
            if (currentDoubleQuoteSequence > longestDoubleQuoteSequence)
                longestDoubleQuoteSequence = currentDoubleQuoteSequence;
        }

        return new string('\"', longestDoubleQuoteSequence < 3 ? 3 : longestDoubleQuoteSequence + 1);
    }

    static string Escape(char x) =>
        x switch
        {
            '\0' => @"\0",
            '\a' => @"\a",
            '\b' => @"\b",
            '\t' => @"\t",
            '\n' => @"\n",
            '\v' => @"\v",
            '\f' => @"\f",
            '\r' => @"\r",
            //'\e' => @"\e", TODO: Applicable in C# 13
            ' ' => " ",
            '"' => @"\""",
            '\'' => @"\'",
            '\\' => @"\\",
            _ when (char.IsControl(x) || char.IsWhiteSpace(x)) => $"\\u{(int)x:X4}",
            _ => x.ToString()
        };

    static string Serialize(Type type)
    {
        var underlyingType = Nullable.GetUnderlyingType(type);

        if (underlyingType != null)
            return $"typeof({TypeName(underlyingType)}?)";

        return $"typeof({TypeName(type)})";
    }

    private static string TypeName(Type x)
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