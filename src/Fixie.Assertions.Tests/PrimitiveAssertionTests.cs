namespace Tests;

class PrimitiveAssertionTests
{
    public void ShouldAssertBools()
    {
        true.ShouldBe(true);
        false.ShouldBe(false);

        Contradiction(true, x => x.ShouldBe(false), "x should be false but was true");
        Contradiction(false, x => x.ShouldBe(true), "x should be true but was false");
    }

    public void ShouldAssertIntegralNumbers()
    {
        sbyte.MinValue.ShouldBe(sbyte.MinValue);
        sbyte.MaxValue.ShouldBe(sbyte.MaxValue);
        Contradiction((sbyte)1, x => x.ShouldBe((sbyte)2), "x should be 2 but was 1");
        
        byte.MinValue.ShouldBe(byte.MinValue);
        byte.MaxValue.ShouldBe(byte.MaxValue);
        Contradiction((byte)2, x => x.ShouldBe((byte)3), "x should be 3 but was 2");
        
        short.MinValue.ShouldBe(short.MinValue);
        short.MaxValue.ShouldBe(short.MaxValue);
        Contradiction((short)3, x => x.ShouldBe((short)4), "x should be 4 but was 3");
        
        ushort.MinValue.ShouldBe(ushort.MinValue);
        ushort.MaxValue.ShouldBe(ushort.MaxValue);
        Contradiction((ushort)4, x => x.ShouldBe((ushort)5), "x should be 5 but was 4");

        int.MinValue.ShouldBe(int.MinValue);
        int.MaxValue.ShouldBe(int.MaxValue);
        Contradiction((int)5, x => x.ShouldBe((int)6), "x should be 6 but was 5");

        uint.MinValue.ShouldBe(uint.MinValue);
        uint.MaxValue.ShouldBe(uint.MaxValue);
        Contradiction((uint)6, x => x.ShouldBe((uint)7), "x should be 7 but was 6");
        
        long.MinValue.ShouldBe(long.MinValue);
        long.MaxValue.ShouldBe(long.MaxValue);
        Contradiction((long)7, x => x.ShouldBe((long)8), "x should be 8 but was 7");

        ulong.MinValue.ShouldBe(ulong.MinValue);
        ulong.MaxValue.ShouldBe(ulong.MaxValue);
        Contradiction((ulong)8, x => x.ShouldBe((ulong)9), "x should be 9 but was 8");

        nint.MinValue.ShouldBe(nint.MinValue);
        nint.MaxValue.ShouldBe(nint.MaxValue);
        Contradiction((nint)9, x => x.ShouldBe((nint)10), "x should be 10 but was 9");

        nuint.MinValue.ShouldBe(nuint.MinValue);
        nuint.MaxValue.ShouldBe(nuint.MaxValue);
        Contradiction((nuint)10, x => x.ShouldBe((nuint)11), "x should be 11 but was 10");

        ((int?)null).ShouldBe(null);
        Contradiction((int?)null, x => x.ShouldBe(1), "x should be 1 but was null");
        Contradiction((int?)1, x => x.ShouldBe(null), "x should be null but was 1");
    }

    public void ShouldAssertFractionalNumbers()
    {
        decimal.MinValue.ShouldBe(decimal.MinValue);
        decimal.MaxValue.ShouldBe(decimal.MaxValue);
        Contradiction((decimal)1, x => x.ShouldBe((decimal)2), "x should be 2 but was 1");

        double.MinValue.ShouldBe(double.MinValue);
        double.MaxValue.ShouldBe(double.MaxValue);
        Contradiction((double)2, x => x.ShouldBe((double)3), "x should be 3 but was 2");

        float.MinValue.ShouldBe(float.MinValue);
        float.MaxValue.ShouldBe(float.MaxValue);
        Contradiction((float)3, x => x.ShouldBe((float)4), "x should be 4 but was 3");
    }

    public void ShouldAssertNullableValueTypes()
    {
        bool? nullableBool = null;

        nullableBool.ShouldBe(null);
        Contradiction(nullableBool, x => x.ShouldBe(false), "x should be false but was null");
        Contradiction(nullableBool, x => x.ShouldBe(true), "x should be true but was null");

        nullableBool = false;

        Contradiction(nullableBool, x => x.ShouldBe(null), "x should be null but was false");
        nullableBool.ShouldBe(false);
        Contradiction(nullableBool, x => x.ShouldBe(true), "x should be true but was false");

        nullableBool = true;

        Contradiction(nullableBool, x => x.ShouldBe(null), "x should be null but was true");
        Contradiction(nullableBool, x => x.ShouldBe(false), "x should be false but was true");
        nullableBool.ShouldBe(true);
    }

    public void ShouldAssertEnums()
    {
        StringSplitOptions.None.ShouldBe(StringSplitOptions.None);
        StringSplitOptions.RemoveEmptyEntries.ShouldBe(StringSplitOptions.RemoveEmptyEntries);
        StringSplitOptions.TrimEntries.ShouldBe(StringSplitOptions.TrimEntries);
        ((StringSplitOptions)int.MaxValue).ShouldBe((StringSplitOptions)int.MaxValue);

        Contradiction(StringSplitOptions.None, x => x.ShouldBe(StringSplitOptions.RemoveEmptyEntries),
            """
            x should be

                System.StringSplitOptions.RemoveEmptyEntries
            
            but was
            
                System.StringSplitOptions.None
            """);
        Contradiction((StringSplitOptions)int.MaxValue, x => x.ShouldBe((StringSplitOptions)int.MinValue),
            """
            x should be
            
                (System.StringSplitOptions)(-2147483648)
            
            but was
            
                (System.StringSplitOptions)2147483647
            """);

        ((StringSplitOptions?)null).ShouldBe(null);
        Contradiction((StringSplitOptions?)null, x => x.ShouldBe(StringSplitOptions.None),
            """
            x should be
            
                System.StringSplitOptions.None
            
            but was
            
                null
            """);
        Contradiction((StringSplitOptions?)StringSplitOptions.TrimEntries, x => x.ShouldBe(null),
            """
            x should be

                null
            
            but was
            
                System.StringSplitOptions.TrimEntries
            """);
    }
}