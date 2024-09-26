using System.Reflection;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static void WritePropertiesLiteral<T>(CsonWriter writer, T value)
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

            SerializeInternal(writer, property.GetValue(value));
        }

        if (any)
            writer.EndItems();

        writer.WriteEndObject();
    }
}