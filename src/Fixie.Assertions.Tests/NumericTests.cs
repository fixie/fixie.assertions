﻿using System.Numerics;

namespace Tests;

class NumericTests
{
    public void ShouldSerializeIntegralNumbers()
    {
        VerifySerialization<sbyte>("-128", "127");
        VerifySerialization<byte>("0", "255");
        VerifySerialization<short>("-32768", "32767");
        VerifySerialization<ushort>("0", "65535");
        VerifySerialization<int>("-2147483648", "2147483647");
        VerifySerialization<uint>("0", "4294967295");
        VerifySerialization<long>("-9223372036854775808", "9223372036854775807");
        VerifySerialization<ulong>("0", "18446744073709551615");
    }

    public void ShouldSerializeNativeIntegralNumbers()
    {
        var nintMinValue = nint.MinValue.ToString();
        (nintMinValue == "-9223372036854775808" ||
         nintMinValue == "-2147483648").ShouldBe(true);

        var nintMaxValue = nint.MaxValue.ToString();
        (nintMaxValue == "9223372036854775807" ||
         nintMaxValue == "2147483647").ShouldBe(true);

        var nuintMaxValue = nuint.MaxValue.ToString();
        (nuintMaxValue == "4294967295" ||
         nuintMaxValue == "18446744073709551615").ShouldBe(true);

        VerifySerialization<nint>(nintMinValue, nintMaxValue);
        VerifySerialization<nuint>("0", nuintMaxValue);
    }

    public void ShouldSerializeFractionalNumbers()
    {
        VerifySerialization<decimal>("-79228162514264337593543950335", "79228162514264337593543950335");
        Serialize(0m).ShouldBe("0");
        Serialize(1m).ShouldBe("1");
        Serialize(0.1m).ShouldBe("0.1");
        Serialize(0.10m).ShouldBe("0.10"); // Trailing zero is preserved.

        VerifySerialization<double>("-1.7976931348623157E+308", "1.7976931348623157E+308");
        Serialize(0d).ShouldBe("0");
        Serialize(1d).ShouldBe("1");
        Serialize(0.1d).ShouldBe("0.1");
        Serialize(0.10d).ShouldBe("0.1");

        VerifySerialization<float>("-3.4028235E+38", "3.4028235E+38");
        Serialize(0f).ShouldBe("0");
        Serialize(1f).ShouldBe("1");
        Serialize(0.1f).ShouldBe("0.1");
        Serialize(0.10f).ShouldBe("0.1");
    }

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
    }

    public void ShouldAssertNativeIntegralNumbers()
    {
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

    static void VerifySerialization<T>(string expectedMin, string expectedMax) where T : struct, INumber<T>
    {
        var actualMin = ReadConstant<T>("MinValue");
        var actualMax = ReadConstant<T>("MaxValue");

        Serialize(actualMin).ShouldBe(expectedMin);
        Serialize(actualMax).ShouldBe(expectedMax);

        // When not null, Nullable<T> serialization should delegate
        // to the serialization scheme of the underlying type.
        Serialize((T?)null).ShouldBe("null");        
        Serialize((T?)actualMin).ShouldBe(expectedMin);
        Serialize((T?)actualMax).ShouldBe(expectedMax);
    }

    static T ReadConstant<T>(string memberName)
    {
        var minValue =
            (typeof(T).GetField(memberName)?.GetValue(null)) ??
            (typeof(T).GetProperty(memberName)?.GetValue(null));

        minValue.ShouldNotBeNull();

        return (T)minValue;
    }
}