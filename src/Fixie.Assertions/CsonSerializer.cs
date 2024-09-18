using System.Text.Json;

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
    }

    public static string Serialize<T>(T value)
        => JsonSerializer.Serialize(value, JsonSerializerOptions);
}