using System.Reflection;

namespace Fixie.Assertions;

partial class CsonSerializer
{
    static void WritePropertiesLiteral<T>(CsonWriter writer, T value)
    {
        var properties =
            typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(property => property.GetIndexParameters().Length == 0);

        writer.WriteItems('{', properties, '}', property =>
        {
            writer.Write(property.Name);
            writer.Write(" = ");
            SerializeInternal(writer, property.GetValue(value));
        });
    }
}