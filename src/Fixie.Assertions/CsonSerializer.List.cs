namespace Fixie.Assertions;

partial class CsonSerializer
{
    static Type? GetEnumerableType(Type typeToConvert)
    {
        if (IsEnumerableT(typeToConvert))
            return typeToConvert;

        return typeToConvert.GetInterfaces().FirstOrDefault(IsEnumerableT);
    }

    static bool IsEnumerableT(Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);

    static void WriteListLiteral<TItem>(CsonWriter writer, IEnumerable<TItem> items)
    {
        writer.WriteItems('[', items, ']', item =>
        {
            SerializeInternal(writer, item);
        });
    }
}