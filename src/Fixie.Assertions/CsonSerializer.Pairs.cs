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

    static void WritePairsLiteral<TKey, TValue>(CsonWriter writer, IEnumerable<KeyValuePair<TKey, TValue>> pairs)
    {
        writer.WriteItems('{', pairs, '}', pair =>
        {
            writer.Write('[');
            SerializeInternal(writer, pair.Key);
            writer.Write("] = ");
            SerializeInternal(writer, pair.Value);
        });
    }
}