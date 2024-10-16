namespace Tests;

class TypeTests
{
    public void ShouldSerializeTypes()
    {
        Serialize((Type?)null).ShouldBe("null");

        Serialize(typeof(Guid)).ShouldBe("typeof(System.Guid)");
        Serialize(typeof(bool)).ShouldBe("typeof(bool)");
        Serialize(typeof(sbyte)).ShouldBe("typeof(sbyte)");
        Serialize(typeof(byte)).ShouldBe("typeof(byte)");
        Serialize(typeof(short)).ShouldBe("typeof(short)");
        Serialize(typeof(ushort)).ShouldBe("typeof(ushort)");
        Serialize(typeof(int)).ShouldBe("typeof(int)");
        Serialize(typeof(uint)).ShouldBe("typeof(uint)");
        Serialize(typeof(long)).ShouldBe("typeof(long)");
        Serialize(typeof(ulong)).ShouldBe("typeof(ulong)");
        Serialize(typeof(nint)).ShouldBe("typeof(nint)");
        Serialize(typeof(nuint)).ShouldBe("typeof(nuint)");
        Serialize(typeof(decimal)).ShouldBe("typeof(decimal)");
        Serialize(typeof(double)).ShouldBe("typeof(double)");
        Serialize(typeof(float)).ShouldBe("typeof(float)");
        Serialize(typeof(char)).ShouldBe("typeof(char)");
        Serialize(typeof(string)).ShouldBe("typeof(string)");
        Serialize(typeof(object)).ShouldBe("typeof(object)");

        Serialize(typeof(Guid?)).ShouldBe("typeof(System.Guid?)");
        Serialize(typeof(int?)).ShouldBe("typeof(int?)");

        Serialize(typeof(Sample)).ShouldBe("typeof(Tests.TypeTests.Sample)");
        
        Serialize(typeof(Tests.TypeTests.Outermost<>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>)");
        Serialize(typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>)");
        Serialize(typeof(Tests.TypeTests.Outermost<int>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<int>)");

        Serialize(typeof(Tests.TypeTests.Outermost<>.Inner<>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.Inner<>)");
        Serialize(typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>)");
        Serialize(typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>>)");

        Serialize(typeof(Tests.TypeTests.Outermost<>.Inner<>.Innermost<>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.Inner<>.Innermost<>)");
        Serialize(typeof(Tests.TypeTests.Outermost<bool>.Inner<Tests.TypeTests.Sample>.Innermost<string>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<bool>.Inner<Tests.TypeTests.Sample>.Innermost<string>)");
        Serialize(typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Outermost<System.TimeSpan>.Inner<Tests.TypeTests.Sample>>.Inner<Tests.TypeTests.Outermost<bool>.Inner<Tests.TypeTests.Sample>.Innermost<TimeSpan>>.Innermost<Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<Tests.TypeTests.Outermost<System.TimeSpan>.Inner<Tests.TypeTests.Sample>>.Inner<Tests.TypeTests.Outermost<bool>.Inner<Tests.TypeTests.Sample>.Innermost<System.TimeSpan>>.Innermost<Tests.TypeTests.Outermost<Tests.TypeTests.Sample>.Inner<int>>)");

        Serialize(typeof(Tests.TypeTests.Outermost<>.InnerEnum))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.InnerEnum)");
        Serialize(typeof(Tests.TypeTests.Outermost<int>.InnerEnum))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<int>.InnerEnum)");

        Serialize(typeof(Tests.TypeTests.Outermost<>.InnerTwo<,>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.InnerTwo<,>)");
        Serialize(typeof(Tests.TypeTests.Outermost<int>.InnerTwo<bool?,string>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<int>.InnerTwo<bool?, string>)");

        Serialize(typeof(Tests.TypeTests.Outermost<>.Nongeneric))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.Nongeneric)");
        Serialize(typeof(Tests.TypeTests.Outermost<>.Nongeneric.MoreGeneric<>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.Nongeneric.MoreGeneric<>)");
        Serialize(typeof(Tests.TypeTests.Outermost<string>.Nongeneric.MoreGeneric<int?>))
            .ShouldBe("typeof(Tests.TypeTests.Outermost<string>.Nongeneric.MoreGeneric<int?>)");            

        // Somewhat surprising, but demonstrates we either have ALL generic
        // type parameters or else ALL specified with concrete types.
        var outerSpecified = typeof(Tests.TypeTests.Outermost<int>);
        var innerTwoOpen = outerSpecified.GetNestedType("InnerTwo`2").ShouldNotBeNull();
        Serialize(innerTwoOpen)
            .ShouldBe("typeof(Tests.TypeTests.Outermost<>.InnerTwo<,>)");

        // MakeGenericType insists all three be provided.
        var innerTwoSpecified = innerTwoOpen.MakeGenericType(typeof(int), typeof(bool), typeof(string));
        Serialize(innerTwoSpecified)
            .ShouldBe("typeof(Tests.TypeTests.Outermost<int>.InnerTwo<bool, string>)");
    }

    public void ShouldAssertTypeEquality()
    {
        typeof(int).ShouldBe(typeof(int));
        
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(GeneralAssertionTests)),
             """
             x should be

                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(Tests.Utility)
             """);
        Contradiction(typeof(bool?), x => x.ShouldBe(typeof(GeneralAssertionTests)),
             """
             x should be
             
                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(bool?)
             """);
        
        Contradiction(typeof(object), x => x.ShouldBe(typeof(GeneralAssertionTests)),
             """
             x should be
             
                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(object)
             """);
        Contradiction(typeof(GeneralAssertionTests), x => x.ShouldBe(typeof(object)),
             """
             x should be
             
                 typeof(object)
             
             but was
             
                 typeof(Tests.GeneralAssertionTests)
             """);

        Contradiction((Type?)null, x => x.ShouldBe(typeof(object)),
             """
             x should be
             
                 typeof(object)
             
             but was
             
                 null
             """);
        Contradiction((Type?)null, x => x.ShouldBe(typeof(Type)),
             """
             x should be
             
                 typeof(System.Type)
             
             but was
             
                 null
             """);
        ((Type?)null).ShouldBe(null);
    }

    public void ShouldAssertValueMatchesTypePattern()
    {
        1.ShouldBe<int>();

        Contradiction(new Sample(), x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 Tests.TypeTests.Sample
             """);
        Contradiction(true, x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 bool
             """);
        
        Contradiction(new object(), x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 object
             """);
        new GeneralAssertionTests().ShouldBe<object>();

        // Just like with the `is` keyword, although expressions may have some known compile time type, null values do not have a type.
        Contradiction((int?)null, x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 null
             """);
        Contradiction((Exception?)null, x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern

                 is Tests.GeneralAssertionTests
             
             but was

                 null
             """);
        Contradiction((GeneralAssertionTests?)null, x => x.ShouldBe<GeneralAssertionTests>(),
             """
             x should match the type pattern

                 is Tests.GeneralAssertionTests

             but was

                 null
             """);

        Exception exception = new DivideByZeroException();
        Exception exceptionAsAbstraction = exception.ShouldBe<Exception>();
        exceptionAsAbstraction.ShouldBe(exception);
        DivideByZeroException exceptionAsConcretion = exception.ShouldBe<DivideByZeroException>();
        exceptionAsConcretion.ShouldBe(exception);
    }

    class Sample;

    class Outermost<TOuter>
    {
        public class Inner<TInner>
        {
            // Deliberately confusing type parameter name to ensure it doesn't
            // arrive in serialized output even while System.TimeSpan does.
            public class Innermost<TimeSpan>
            {
            }
        }

        public class InnerTwo<T1, T2>
        {
        }

        public enum InnerEnum
        {
        }

        public class Nongeneric
        {
            public class MoreGeneric<TMore>
            {
            }
        }
    }
}