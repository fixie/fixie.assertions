using System.Reflection;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static void WritePropertiesLiteral<T>(CsonWriter writer, T value)
    {
        writer.Write('{');

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

            SerializeInternal(writer, property.Name);
            writer.Write(": ");
            SerializeInternal(writer, property.GetValue(value));
        }

        if (any)
            writer.EndItems();

        writer.Write('}');
    }
}