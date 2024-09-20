namespace Tests;

class CsonScalarTests
{
    public void ShouldSerializeBools()
    {
        Serialize(true).ShouldBe("true");
        Serialize(false).ShouldBe("false");
    }

    public void ShouldSerializeIntegralNumbers()
    {
        Serialize(sbyte.MinValue).ShouldBe("-128");
        Serialize(sbyte.MaxValue).ShouldBe("127");
        
        Serialize(byte.MinValue).ShouldBe("0");
        Serialize(byte.MaxValue).ShouldBe("255");
        
        Serialize(short.MinValue).ShouldBe("-32768");
        Serialize(short.MaxValue).ShouldBe("32767");
        
        Serialize(ushort.MinValue).ShouldBe("0");
        Serialize(ushort.MaxValue).ShouldBe("65535");

        Serialize(int.MinValue).ShouldBe("-2147483648");
        Serialize(int.MaxValue).ShouldBe("2147483647");

        Serialize(uint.MinValue).ShouldBe("0");
        Serialize(uint.MaxValue).ShouldBe("4294967295");
        
        Serialize(long.MinValue).ShouldBe("-9223372036854775808");
        Serialize(long.MaxValue).ShouldBe("9223372036854775807");

        Serialize(ulong.MinValue).ShouldBe("0");
        Serialize(ulong.MaxValue).ShouldBe("18446744073709551615");

        Serialize((nint)(-1)).ShouldBe("-1");
        Serialize((nint)0).ShouldBe("0");
        Serialize((nint)1).ShouldBe("1");

        Serialize((nuint)0).ShouldBe("0");
        Serialize((nuint)1).ShouldBe("1");
    }

    public void ShouldSerializeFractionalNumbers()
    {
        Serialize(decimal.MinValue).ShouldBe("-79228162514264337593543950335");
        Serialize(0m).ShouldBe("0");
        Serialize(1m).ShouldBe("1");
        Serialize(0.1m).ShouldBe("0.1");
        Serialize(0.10m).ShouldBe("0.10"); // Trailing zero is preserved.
        Serialize(decimal.MaxValue).ShouldBe("79228162514264337593543950335");

        Serialize(double.MinValue).ShouldBe("-1.7976931348623157E+308");
        Serialize(0d).ShouldBe("0");
        Serialize(1d).ShouldBe("1");
        Serialize(0.1d).ShouldBe("0.1");
        Serialize(0.10d).ShouldBe("0.1");
        Serialize(double.MaxValue).ShouldBe("1.7976931348623157E+308");

        Serialize(float.MinValue).ShouldBe("-3.4028235E+38");
        Serialize(0f).ShouldBe("0");
        Serialize(1f).ShouldBe("1");
        Serialize(0.1f).ShouldBe("0.1");
        Serialize(0.10f).ShouldBe("0.1");
        Serialize(float.MaxValue).ShouldBe("3.4028235E+38");
    }

    public void ShouldSerializeEnums()
    {
        Serialize((StringSplitOptions?)null)
            .ShouldBe("null");

        Serialize(StringSplitOptions.None)
            .ShouldBe("System.StringSplitOptions.None");

        Serialize(StringSplitOptions.RemoveEmptyEntries)
            .ShouldBe("System.StringSplitOptions.RemoveEmptyEntries");

        Serialize(StringSplitOptions.TrimEntries)
            .ShouldBe("System.StringSplitOptions.TrimEntries");

        Serialize((StringSplitOptions)int.MinValue)
            .ShouldBe("(System.StringSplitOptions)(-2147483648)");

        Serialize((StringSplitOptions)int.MaxValue)
            .ShouldBe("(System.StringSplitOptions)2147483647");
    }

    public void ShouldSerializeNullableValueTypes()
    {
        bool? nullableBool = null;

        Serialize(nullableBool).ShouldBe("null");

        nullableBool = false;

        Serialize(nullableBool).ShouldBe("false");

        nullableBool = true;

        Serialize(nullableBool).ShouldBe("true");
    }

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

    static string Serialize<T>(T value)
        => CsonSerializer.Serialize(value);
}