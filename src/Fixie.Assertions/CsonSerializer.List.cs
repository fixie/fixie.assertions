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

    static void WriteListLiteral<TItem>(CsonWriter writer, IEnumerable<TItem> value)
    {
        writer.Write('[');

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

            SerializeInternal(writer, item);
        }

        if (any)
            writer.EndItems();

        writer.Write(']');
    }
}