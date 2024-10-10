using System.Text;

namespace Fixie.Assertions;

static class StringUtilities
{
   public static string TypeName(Type type)
    {
        var keyword = type switch
        {
            _ when type == typeof(bool) => "bool",
            _ when type == typeof(sbyte) => "sbyte",
            _ when type == typeof(byte) => "byte",
            _ when type == typeof(short) => "short",
            _ when type == typeof(ushort) => "ushort",
            _ when type == typeof(int) => "int",
            _ when type == typeof(uint) => "uint",
            _ when type == typeof(long) => "long",
            _ when type == typeof(ulong) => "ulong",
            _ when type == typeof(nint) => "nint",
            _ when type == typeof(nuint) => "nuint",
            _ when type == typeof(decimal) => "decimal",
            _ when type == typeof(double) => "double",
            _ when type == typeof(float) => "float",
            _ when type == typeof(char) => "char",
            _ when type == typeof(string) => "string",
            _ when type == typeof(object) => "object",
            _ => null
        };

        if (keyword != null)
            return keyword;

        if (type.IsGenericTypeParameter)
            return "";

        // When we have a combination of generics and nesting, where we may have 0..N generic type parameters
        // introduced at each nesting level, the behavior of Type is surprising. At each level, GetGenericArguments()
        // returns an array with items corresponding with ALL of the generic type parameters in the FULL name up
        // to that point in the nesting, rather than only the number directly found on the nested type under
        // inspection. Also, the contents of GetGenericArguments() at each intermediate level will describe the
        // unspecified placeholder names, even if the overall incoming Type we're processing has real types
        // specified.
        //
        // So, we may only trust the contents of the GetGenericArguments() array for the incoming Type we're
        // processing, but we need to inspect the counts of each nesting level's GetGenericArguments() array as we
        // go in order to know which of the trustworthy Types to place within <...> at each level.

        var argumentsToApply = type.GetGenericArguments();

        var scope = new Stack<Type>();
        var walk = type;        
        while(walk != null)
        {
            scope.Push(walk);
            walk = walk.DeclaringType;
        }

        var result = new StringBuilder();
        result.Append(type.Namespace);

        var skip = 0;
        foreach (var link in scope)
        {
            var take = link.GetGenericArguments().Length - skip;

            result.Append('.');

            if (take == 0)
            {
                result.Append(link.Name);
            }
            else
            {
                var nameWithoutCount = link.Name.Substring(0, link.Name.IndexOf('`'));
                result.Append(nameWithoutCount);
                result.Append('<');

                bool first = true;
                foreach (var argument in argumentsToApply.Skip(skip).Take(take))
                {
                    if (!first)
                        result.Append(',');

                    if (!type.IsGenericTypeParameter)
                        result.Append(TypeName(argument));

                    first = false;
                }

                result.Append('>');
            }
                
            skip += take;
        }

        return result.ToString();
    }
}