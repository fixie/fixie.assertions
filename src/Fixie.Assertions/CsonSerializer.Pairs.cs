namespace Fixie.Assertions;

partial class CsonSerializer
{
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

    static void WritePairsLiteral<TKey, TValue>(CsonWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> value)
    {
        writer.Write('{');

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

            SerializeInternal(writer, item.Key);
            writer.Write(": ");
            SerializeInternal(writer, item.Value);
        }

        if (any)
            writer.EndItems();

        writer.Write('}');
    }
}