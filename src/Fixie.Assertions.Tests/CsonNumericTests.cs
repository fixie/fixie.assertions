using System.Numerics;

namespace Tests;

class CsonNumericTests
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