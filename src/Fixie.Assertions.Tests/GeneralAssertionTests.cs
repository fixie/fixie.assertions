namespace Fixie.Assertions.Tests;

class GeneralAssertionTests
{
    public void ShouldAssertEquatables()
    {
        HttpMethod.Post.ShouldBe(HttpMethod.Post);
        Contradiction(HttpMethod.Post, x => x.ShouldBe(HttpMethod.Get), "x should be GET but was POST");
    }

    public void ShouldAssertObjects()
    {
        var objectA = new SampleA();
        var objectB = new SampleB();

        objectA.ShouldBe(objectA);
        objectB.ShouldBe(objectB);

        Contradiction(objectB, x => x.ShouldBe((object?)null),
            $"x should be null but was {FullName<SampleB>()}");
        Contradiction(objectB, x => x.ShouldBe(objectA),
            $"x should be {FullName<SampleA>()} but was {FullName<SampleB>()}");
        Contradiction(objectA, x => x.ShouldBe(objectB),
            $"x should be {FullName<SampleB>()} but was {FullName<SampleA>()}");
    }

    public void ShouldAssertNulls()
    {
        object? nullObject = null;
        object nonNullObject = new SampleA();

        nullObject.ShouldBe(null);
        nonNullObject.ShouldNotBeNull();

        Contradiction((object?)null, x => x.ShouldBe(nonNullObject),
            $"x should be {FullName<SampleA>()} but was null");
        Contradiction(nonNullObject, x => x.ShouldBe(null),
            $"x should be null but was {FullName<SampleA>()}");
        Contradiction((object?)null, x => x.ShouldNotBeNull(),
            "x should not be null but was null");
    }

    public void ShouldAssertTypes()
    {
        typeof(int).ShouldBe(typeof(int));
        typeof(char).ShouldBe(typeof(char));
        Contradiction(typeof(Utility), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(Fixie.Assertions.Tests.Utility)");
        Contradiction(typeof(bool), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(bool)");
        Contradiction(typeof(sbyte), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(sbyte)");
        Contradiction(typeof(byte), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(byte)");
        Contradiction(typeof(short), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(short)");
        Contradiction(typeof(ushort), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(ushort)");
        Contradiction(typeof(int), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(int)");
        Contradiction(typeof(uint), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(uint)");
        Contradiction(typeof(long), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(long)");
        Contradiction(typeof(ulong), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(ulong)");
        Contradiction(typeof(nint), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(nint)");
        Contradiction(typeof(nuint), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(nuint)");
        Contradiction(typeof(decimal), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(decimal)");
        Contradiction(typeof(double), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(double)");
        Contradiction(typeof(float), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(float)");
        Contradiction(typeof(char), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(char)");
        Contradiction(typeof(string), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(string)");
        Contradiction(typeof(object), x => x.ShouldBe(typeof(GeneralAssertionTests)), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(object)");

        1.ShouldBe<int>();
        'A'.ShouldBe<char>();
        Exception exception = new DivideByZeroException();
        DivideByZeroException typedException = exception.ShouldBe<DivideByZeroException>();
        Contradiction(new SampleA(), x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof({FullName<SampleA>()})");
        Contradiction(true, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(bool)");
        Contradiction((sbyte)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(sbyte)");
        Contradiction((byte)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(byte)");
        Contradiction((short)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(short)");
        Contradiction((ushort)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(ushort)");
        Contradiction((int)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(int)");
        Contradiction((uint)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(uint)");
        Contradiction((long)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(long)");
        Contradiction((ulong)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(ulong)");
        Contradiction((nint)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(nint)");
        Contradiction((nuint)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(nuint)");
        Contradiction((decimal)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(decimal)");
        Contradiction((double)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(double)");
        Contradiction((float)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(float)");
        Contradiction((char)1, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(char)");
        Contradiction("A", x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(string)");
        Contradiction(new object(), x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was typeof(object)");
        Contradiction((Exception?)null, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was null");
        Contradiction((GeneralAssertionTests?)null, x => x.ShouldBe<GeneralAssertionTests>(), $"x should be typeof({FullName<GeneralAssertionTests>()}) but was null");
    }

    class SampleA;
    class SampleB;
}