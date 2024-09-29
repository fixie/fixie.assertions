namespace Tests;

class PrimitiveAssertionTests
{
    public void ShouldAssertIntegralNumbers()
    {
        sbyte.MinValue.ShouldBe(sbyte.MinValue);
        sbyte.MaxValue.ShouldBe(sbyte.MaxValue);
        Contradiction((sbyte)1, x => x.ShouldBe((sbyte)2), "x should be 2 but was 1");
        ((sbyte?)null).ShouldBe(null);
        Contradiction((sbyte?)null, x => x.ShouldBe((sbyte)1), "x should be 1 but was null");
        Contradiction((sbyte?)1, x => x.ShouldBe(null), "x should be null but was 1");
        
        byte.MinValue.ShouldBe(byte.MinValue);
        byte.MaxValue.ShouldBe(byte.MaxValue);
        Contradiction((byte)2, x => x.ShouldBe((byte)3), "x should be 3 but was 2");
        ((byte?)null).ShouldBe(null);
        Contradiction((byte?)null, x => x.ShouldBe((byte)2), "x should be 2 but was null");
        Contradiction((byte?)2, x => x.ShouldBe(null), "x should be null but was 2");
        
        short.MinValue.ShouldBe(short.MinValue);
        short.MaxValue.ShouldBe(short.MaxValue);
        Contradiction((short)3, x => x.ShouldBe((short)4), "x should be 4 but was 3");
        ((short?)null).ShouldBe(null);
        Contradiction((short?)null, x => x.ShouldBe((short)3), "x should be 3 but was null");
        Contradiction((short?)3, x => x.ShouldBe(null), "x should be null but was 3");
        
        ushort.MinValue.ShouldBe(ushort.MinValue);
        ushort.MaxValue.ShouldBe(ushort.MaxValue);
        Contradiction((ushort)4, x => x.ShouldBe((ushort)5), "x should be 5 but was 4");
        ((ushort?)null).ShouldBe(null);
        Contradiction((ushort?)null, x => x.ShouldBe((ushort)4), "x should be 4 but was null");
        Contradiction((ushort?)4, x => x.ShouldBe(null), "x should be null but was 4");

        int.MinValue.ShouldBe(int.MinValue);
        int.MaxValue.ShouldBe(int.MaxValue);
        Contradiction((int)5, x => x.ShouldBe((int)6), "x should be 6 but was 5");
        ((int?)null).ShouldBe(null);
        Contradiction((int?)null, x => x.ShouldBe((int)5), "x should be 5 but was null");
        Contradiction((int?)5, x => x.ShouldBe(null), "x should be null but was 5");

        uint.MinValue.ShouldBe(uint.MinValue);
        uint.MaxValue.ShouldBe(uint.MaxValue);
        Contradiction((uint)6, x => x.ShouldBe((uint)7), "x should be 7 but was 6");
        ((uint?)null).ShouldBe(null);
        Contradiction((uint?)null, x => x.ShouldBe((uint)6), "x should be 6 but was null");
        Contradiction((uint?)6, x => x.ShouldBe(null), "x should be null but was 6");
        
        long.MinValue.ShouldBe(long.MinValue);
        long.MaxValue.ShouldBe(long.MaxValue);
        Contradiction((long)7, x => x.ShouldBe((long)8), "x should be 8 but was 7");
        ((long?)null).ShouldBe(null);
        Contradiction((long?)null, x => x.ShouldBe((long)7), "x should be 7 but was null");
        Contradiction((long?)7, x => x.ShouldBe(null), "x should be null but was 7");

        ulong.MinValue.ShouldBe(ulong.MinValue);
        ulong.MaxValue.ShouldBe(ulong.MaxValue);
        Contradiction((ulong)8, x => x.ShouldBe((ulong)9), "x should be 9 but was 8");
        ((ulong?)null).ShouldBe(null);
        Contradiction((ulong?)null, x => x.ShouldBe((ulong)8), "x should be 8 but was null");
        Contradiction((ulong?)8, x => x.ShouldBe(null), "x should be null but was 8");

        nint.MinValue.ShouldBe(nint.MinValue);
        nint.MaxValue.ShouldBe(nint.MaxValue);
        Contradiction((nint)9, x => x.ShouldBe((nint)10), "x should be 10 but was 9");
        ((nint?)null).ShouldBe(null);
        Contradiction((nint?)null, x => x.ShouldBe((nint)9), "x should be 9 but was null");
        Contradiction((nint?)9, x => x.ShouldBe(null), "x should be null but was 9");

        nuint.MinValue.ShouldBe(nuint.MinValue);
        nuint.MaxValue.ShouldBe(nuint.MaxValue);
        Contradiction((nuint)10, x => x.ShouldBe((nuint)11), "x should be 11 but was 10");
        ((nuint?)null).ShouldBe(null);
        Contradiction((nuint?)null, x => x.ShouldBe((nuint)10), "x should be 10 but was null");
        Contradiction((nuint?)10, x => x.ShouldBe(null), "x should be null but was 10");
    }

    public void ShouldAssertFractionalNumbers()
    {
        decimal.MinValue.ShouldBe(decimal.MinValue);
        decimal.MaxValue.ShouldBe(decimal.MaxValue);
        Contradiction((decimal)1, x => x.ShouldBe((decimal)2), "x should be 2 but was 1");
        ((decimal?)null).ShouldBe(null);
        Contradiction((decimal?)null, x => x.ShouldBe((decimal)1), "x should be 1 but was null");
        Contradiction((decimal?)1, x => x.ShouldBe(null), "x should be null but was 1");

        double.MinValue.ShouldBe(double.MinValue);
        double.MaxValue.ShouldBe(double.MaxValue);
        Contradiction((double)2, x => x.ShouldBe((double)3), "x should be 3 but was 2");
        ((double?)null).ShouldBe(null);
        Contradiction((double?)null, x => x.ShouldBe((double)2), "x should be 2 but was null");
        Contradiction((double?)2, x => x.ShouldBe(null), "x should be null but was 2");

        float.MinValue.ShouldBe(float.MinValue);
        float.MaxValue.ShouldBe(float.MaxValue);
        Contradiction((float)3, x => x.ShouldBe((float)4), "x should be 4 but was 3");
        ((float?)null).ShouldBe(null);
        Contradiction((float?)null, x => x.ShouldBe((float)3), "x should be 3 but was null");
        Contradiction((float?)3, x => x.ShouldBe(null), "x should be null but was 3");
    }
}