using static Fixie.Assertions.CsonSerializer;

namespace Tests;

class CsonScalarTests
{
    public void ShouldSerializeBools()
    {
        Serialize(true).ShouldBe("true");
        Serialize(false).ShouldBe("false");
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
}