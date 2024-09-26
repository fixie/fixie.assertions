namespace Fixie.Assertions;

partial class CsonSerializer
{
    static void WriteEnumLiteral<TValue>(CsonWriter writer, TValue value) where TValue : struct, Enum
        => writer.WriteRawValue(SerializeEnum(value));

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