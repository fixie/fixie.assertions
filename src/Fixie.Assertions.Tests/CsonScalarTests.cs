namespace Tests;

class CsonScalarTests
{
    public void ShouldSerializeBools()
    {
        Serialize(true).ShouldBe("true");
        Serialize(false).ShouldBe("false");

        Serialize((bool?)null).ShouldBe("null");        
        Serialize((bool?)true).ShouldBe("true");
        Serialize((bool?)false).ShouldBe("false");
    }

    public void ShouldSerializeEnums()
    {
        var prefix = "System.StringSplitOptions";

        Serialize(StringSplitOptions.None).ShouldBe(prefix+".None");
        Serialize(StringSplitOptions.RemoveEmptyEntries).ShouldBe(prefix+".RemoveEmptyEntries");
        Serialize(StringSplitOptions.TrimEntries).ShouldBe(prefix+".TrimEntries");

        Serialize((StringSplitOptions)int.MinValue).ShouldBe($"({prefix})(-2147483648)");
        Serialize((StringSplitOptions)int.MaxValue).ShouldBe($"({prefix})2147483647");

        Serialize((StringSplitOptions?)null).ShouldBe("null");
        Serialize((StringSplitOptions?)StringSplitOptions.None).ShouldBe(prefix+".None");
        Serialize((StringSplitOptions?)StringSplitOptions.RemoveEmptyEntries).ShouldBe(prefix+".RemoveEmptyEntries");
        Serialize((StringSplitOptions?)StringSplitOptions.TrimEntries).ShouldBe(prefix+".TrimEntries");
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