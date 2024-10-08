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
    }

    public void ShouldAssertTypeEquality()
    {
        typeof(int).ShouldBe(typeof(int));
        
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(GeneralAssertionTests)),
            $"""
             x should be

                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(Tests.Utility)
             """);
        Contradiction(typeof(bool?), x => x.ShouldBe(typeof(GeneralAssertionTests)),
            $"""
             x should be
             
                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(bool?)
             """);
        
        Contradiction(typeof(object), x => x.ShouldBe(typeof(GeneralAssertionTests)),
            $"""
             x should be
             
                 typeof(Tests.GeneralAssertionTests)
             
             but was
             
                 typeof(object)
             """);
        Contradiction(typeof(GeneralAssertionTests), x => x.ShouldBe(typeof(object)),
            $"""
             x should be
             
                 typeof(object)
             
             but was
             
                 typeof(Tests.GeneralAssertionTests)
             """);

        Contradiction((Type?)null, x => x.ShouldBe(typeof(object)),
            $"""
             x should be
             
                 typeof(object)
             
             but was
             
                 null
             """);
        Contradiction((Type?)null, x => x.ShouldBe(typeof(Type)),
            $"""
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
            $"""
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 Tests.TypeTests+Sample
             """);
        Contradiction(true, x => x.ShouldBe<GeneralAssertionTests>(),
            $"""
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 bool
             """);
        
        Contradiction(new object(), x => x.ShouldBe<GeneralAssertionTests>(),
            $"""
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 object
             """);
        new GeneralAssertionTests().ShouldBe<object>();

        // Just like with the `is` keyword, although expressions may have some known compile time type, null values do not have a type.
        Contradiction((int?)null, x => x.ShouldBe<GeneralAssertionTests>(),
            $"""
             x should match the type pattern
             
                 is Tests.GeneralAssertionTests
             
             but was
             
                 null
             """);
        Contradiction((Exception?)null, x => x.ShouldBe<GeneralAssertionTests>(),
            $"""
             x should match the type pattern

                 is Tests.GeneralAssertionTests
             
             but was

                 null
             """);
        Contradiction((GeneralAssertionTests?)null, x => x.ShouldBe<GeneralAssertionTests>(),
            $"""
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
}