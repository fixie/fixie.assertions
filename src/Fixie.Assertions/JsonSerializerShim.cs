namespace Fixie.Assertions;

ref partial struct Utf8JsonReader
{
}

class Utf8JsonWriter
{
    public void WriteRawValue(string json, bool skipInputValidation = false)
    {
    }

    public void WriteStringValue(string? value)
    {
    }

    public void WriteStartArray()
    {
    }

    public void WriteEndArray()
    {
    }

    public void WriteStartObject()
    {
    }

    public void WritePropertyName(string propertyName)
    {
    }

    public void WriteEndObject()
    {
    }
}

class JsonSerializer
{
    public static string Serialize<TValue>(TValue value, JsonSerializerOptions? options = null)
        => throw new NotImplementedException();

    public static void Serialize<TValue>(Utf8JsonWriter writer, TValue value, JsonSerializerOptions? options = null)
    {
    }
}

abstract class JsonConverter<T> : JsonConverter
{
        public override bool CanConvert(Type typeToConvert)
            => throw new NotImplementedException();

        public abstract void Write(
            Utf8JsonWriter writer,
#nullable disable // T may or may not be nullable depending on the derived converter's HandleNull override.
            T value,
#nullable restore
            JsonSerializerOptions options);

        public abstract T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options);
}

abstract class JsonConverter
{
    public abstract bool CanConvert(Type typeToConvert);
}

abstract class JsonConverterFactory : JsonConverter
{
    public abstract JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options);
}

class JsonSerializerOptions
{
    public List<JsonConverter> Converters { get; } = [];
    public bool WriteIndented { get; init; }
}

class JsonException : Exception
{
}

abstract class JsonAttribute : Attribute
{
}

public enum JsonIgnoreCondition
{
    WhenWritingNull
}

class JsonIgnoreAttribute : JsonAttribute
{
    public JsonIgnoreCondition Condition { get; set; }
}

class JsonPropertyNameAttribute : JsonAttribute
{
    public JsonPropertyNameAttribute(string name)
    {
    }
}

class JsonIncludeAttribute : JsonAttribute
{
}

class JsonConverterAttribute : JsonAttribute
{
    public JsonConverterAttribute(Type converterType)
    {
    }
}

class JsonExtensionDataAttribute : JsonAttribute
{
}

readonly struct JsonElement{ }

class JsonDocument
{
    public static JsonDocument Parse(string json)
        => throw new NotImplementedException();

    public JsonElement RootElement
        => throw new NotImplementedException();
}
