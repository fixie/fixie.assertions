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

        if (IsNumeric(type))
            return CsonSerializer.Serialize(any);

        return any.ToString() ?? any.GetType().ToString();
    }

    static string Serialize(Array items)
    {
        if (items.Length == 0)
            return "[]";

        var formattedItems = string.Join("," + NewLine, items.Cast<object>().Select(arg => "    " + Serialize(arg)));

        return $"[{NewLine}{formattedItems}{NewLine}]";
    }

    static bool IsNumeric(Type type)
    {
        if (type == typeof(sbyte)) return true;
        if (type == typeof(byte)) return true;
        if (type == typeof(short)) return true;
        if (type == typeof(ushort)) return true;
        if (type == typeof(int)) return true;
        if (type == typeof(uint)) return true;
        if (type == typeof(long)) return true;
        if (type == typeof(ulong)) return true;
        if (type == typeof(nint)) return true;
        if (type == typeof(nuint)) return true;
        if (type == typeof(decimal)) return true;
        if (type == typeof(double)) return true;
        if (type == typeof(float)) return true;

        return false;
    }
}