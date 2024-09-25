using System.Diagnostics;
using System.Reflection;
using static Fixie.Assertions.StringUtilities;
using static System.Environment;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static readonly CsonSerializerOptions JsonSerializerOptions;

    static CsonSerializer()
    {
        JsonSerializerOptions = new CsonSerializerOptions();

        JsonSerializerOptions.Converters.Add(new EnumLiteralFactory());
        JsonSerializerOptions.Converters.Add(new PairsLiteralFactory());
        JsonSerializerOptions.Converters.Add(new ListLiteralFactory());
        JsonSerializerOptions.Converters.Add(new PropertiesLiteralFactory());
    }

    public static string Serialize<T>(T value)
        => Serialize(value, JsonSerializerOptions);

    class EnumLiteral<T> : CsonConverter<T> where T : struct, Enum
    {
        public override void Write(CsonWriter writer, T value, CsonSerializerOptions options)
            => writer.WriteRawValue(SerializeEnum(value));
    }

    class EnumLiteralFactory : CsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => typeToConvert.IsEnum;

        public override CsonConverter CreateConverter(Type typeToConvert)
        {
            Type converterType = typeof(EnumLiteral<>).MakeGenericType(typeToConvert);
            return (CsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class PairsLiteralFactory : CsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => GetPairType(typeToConvert) != null;

        public override CsonConverter CreateConverter(Type typeToConvert)
        {
            var pairType = GetPairType(typeToConvert) ?? throw new UnreachableException();

            Type converterType = typeof(PairsLiteral<,>).MakeGenericType(pairType.GetGenericArguments());
            return (CsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class PairsLiteral<TKey, TValue> : CsonConverter<IEnumerable<KeyValuePair<TKey, TValue>>>
    {
        public override void Write(CsonWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> value, CsonSerializerOptions options)
        {
            writer.WriteStartObject();

            bool any = false;
            foreach (var item in value)
            {
                if (!any)
                {
                    writer.StartItems();
                    any = true;
                }
                else
                    writer.WriteItemSeparator();

                writer.WritePropertyName(item.Key?.ToString()!);

                SerializeInternal(writer, item.Value, options);
            }

            if (any)
                writer.EndItems();

            writer.WriteEndObject();
        }
    }

    class ListLiteralFactory : CsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => GetEnumerableType(typeToConvert) != null;

        public override CsonConverter CreateConverter(Type typeToConvert)
        {
            var enumerableType = GetEnumerableType(typeToConvert) ?? throw new UnreachableException();
            var itemType = enumerableType.GetGenericArguments()[0];

            Type converterType = typeof(ListLiteral<>).MakeGenericType(itemType);
            return (CsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class ListLiteral<T> : CsonConverter<IEnumerable<T>>
    {
        public override void Write(CsonWriter writer, IEnumerable<T> value, CsonSerializerOptions options)
        {
            writer.WriteStartArray();

            bool any = false;
            foreach (var item in value)
            {
                if (!any)
                {
                    writer.StartItems();
                    any = true;
                }
                else
                    writer.WriteItemSeparator();

                SerializeInternal(writer, item, options);
            }

            if (any)
                writer.EndItems();

            writer.WriteEndArray();
        }
    }

    class PropertiesLiteralFactory : CsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
            => true;

        public override CsonConverter CreateConverter(Type typeToConvert)
        {
            Type converterType = typeof(PropertiesLiteral<>).MakeGenericType(typeToConvert);
            return (CsonConverter)Activator.CreateInstance(converterType)!;
        }
    }

    class PropertiesLiteral<T> : CsonConverter<T>
    {
        public override void Write(CsonWriter writer, T value, CsonSerializerOptions options)
        {
            writer.WriteStartObject();

            bool any = false;
            foreach (var property in typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.GetIndexParameters().Length > 0)
                    continue;

                if (!any)
                {
                    writer.StartItems();
                    any = true;
                }
                else
                    writer.WriteItemSeparator();

                writer.WritePropertyName(property.Name);

                SerializeInternal(writer, property.GetValue(value), options);
            }

            if (any)
                writer.EndItems();

            writer.WriteEndObject();
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

    static Type? GetPairType(Type typeToConvert)
    {
        var enumerableType = GetEnumerableType(typeToConvert);
            
        if (enumerableType != null)
        {
            var itemType = enumerableType.GetGenericArguments()[0];

            if (itemType.IsGenericType &&
                itemType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
                return itemType;
        }

        return null;
    }

    static string Serialize(bool x) => x ? "true" : "false";

    static string Serialize(char x) => $"'{Escape(x)}'";

    static string Serialize(Guid x) => Serialize(x.ToString());

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

    static string SerializeEnum<T>(T value) where T : struct, Enum
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