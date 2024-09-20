using static System.Environment;

namespace Fixie.Assertions;

static class Serialization
{
    public static string Serialize(object? any)
    {
        if (any == null) return CsonSerializer.Serialize(any);

        var type = any.GetType();

        if (type == typeof(bool))
            return CsonSerializer.Serialize((bool)any);

        if (type == typeof(char))
            return CsonSerializer.Serialize((char)any);

        if (type == typeof(string))
            return CsonSerializer.Serialize((string)any);

        if (type.IsArray)
            return Serialize((Array)any);

        if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
            return CsonSerializer.Serialize((Type)any);

        if (type.IsEnum)
            return CsonSerializer.Serialize(any);

        return any.ToString() ?? any.GetType().ToString();
    }

    static string Serialize(Array items)
    {
        var formattedItems = string.Join("," + NewLine, items.Cast<object>().Select(arg => "    " + Serialize(arg)));

        return $"[{NewLine}{formattedItems}{NewLine}]";
    }
}