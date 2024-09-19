using static System.Environment;

namespace Tests;

class CsonSerializerTests
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

    public void ShouldSerializeChars()
    {
        Serialize((char?)null)
            .ShouldBe("null");

        Serialize('a')
            .ShouldBe("""
                      "a"
                      """);

        Serialize('☺')
            .ShouldBe("""
                      "\u263A"
                      """);
        
        // Escape Sequence: Null
        Serialize('\u0000')
            .ShouldBe("""
                      "\u0000"
                      """);
        Serialize('\0')
            .ShouldBe("""
                      "\u0000"
                      """);

        // Escape Sequence: Alert
        Serialize('\u0007')
            .ShouldBe("""
                      "\u0007"
                      """);
        Serialize('\a')
            .ShouldBe("""
                      "\u0007"
                      """);

        // Escape Sequence: Backspace
        Serialize('\u0008')
            .ShouldBe("""
                      "\b"
                      """);
        Serialize('\b')
            .ShouldBe("""
                      "\b"
                      """);

        // Escape Sequence: Horizontal tab
        Serialize('\u0009')
            .ShouldBe("""
                      "\t"
                      """);
        Serialize('\t')
            .ShouldBe("""
                      "\t"
                      """);

        // Escape Sequence: New line
        Serialize('\u000A')
            .ShouldBe("""
                      "\n"
                      """);
        Serialize('\n')
            .ShouldBe("""
                      "\n"
                      """);

        // Escape Sequence: Vertical tab
        Serialize('\u000B')
            .ShouldBe("""
                      "\u000B"
                      """);
        Serialize('\v')
            .ShouldBe("""
                      "\u000B"
                      """);

        // Escape Sequence: Form feed
        Serialize('\u000C')
            .ShouldBe("""
                      "\f"
                      """);
        Serialize('\f')
            .ShouldBe("""
                      "\f"
                      """);

        // Escape Sequence: Carriage return
        Serialize('\u000D')
            .ShouldBe("""
                      "\r"
                      """);
        Serialize('\r')
            .ShouldBe("""
                      "\r"
                      """);

        // TODO: Applicable in C# 13
        // Escape Sequence: Escape
        // Serialize('\u001B')
        //     .ShouldBe("""
        //               "\e"
        //               """);
        // Serialize('\e')
        //     .ShouldBe("""
        //               "\e"
        //               """);

        // Literal Space
        Serialize('\u0020')
            .ShouldBe("""
                      " "
                      """);
        Serialize(' ')
            .ShouldBe("""
                      " "
                      """);

        // Escape Sequence: Double quote
        Serialize('\u0022')
            .ShouldBe("""
                      "\u0022"
                      """);
        Serialize('\"')
            .ShouldBe("""
                      "\u0022"
                      """);

        // Escape Sequence: Single quote
        Serialize('\u0027')
            .ShouldBe("""
                      "\u0027"
                      """);
        Serialize('\'')
            .ShouldBe("""
                      "\u0027"
                      """);

        // Escape Sequence: Backslash
        Serialize('\u005C')
            .ShouldBe("""
                      "\\"
                      """);
        Serialize('\\')
            .ShouldBe("""
                      "\\"
                      """);

        foreach (var c in UnicodeEscapedCharacters())
            Serialize(c)
                .ShouldBe($"""
                           "\u{(int)c:X4}"
                           """);
    }

    public void ShouldSerializeStrings()
    {
        Serialize((string?)null)
            .ShouldBe("null");

        Serialize("a☺")
            .ShouldBe("""
                      "a\u263A"
                      """);

        Serialize("\u0020 ")
            .ShouldBe("""
                      "  "
                      """);

        Serialize("\u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r")
            .ShouldBe("""
                      "\u0000\u0000 \u0007\u0007 \b\b \t\t \n\n \r\r"
                      """);

        // TODO: In C# 13, include \u001B\e becoming \e\e
        Serialize("\u000C\f \u000B\v \u0022\" \u0027\' \u005C\\")
            .ShouldBe("""
                      "\f\f \u000B\u000B \u0022\u0022 \u0027\u0027 \\\\"
                      """);

        foreach (var c in UnicodeEscapedCharacters())
            Serialize(c.ToString())
                .ShouldBe($"""
                           "\u{(int)c:X4}"
                           """);
    }

    public void ShouldSerializeMultilineStrings()
    {
        var newLineEscaped = NewLine.Replace("\r", "\\r").Replace("\n", "\\n");

        var simple =
            """
            Line 1
            Line 2
            Line 3
            Line 4
            """;

        Serialize(simple)
            .ShouldBe($"""
                       "Line 1{newLineEscaped}Line 2{newLineEscaped}Line 3{newLineEscaped}Line 4"
                       """);

        var mixedLineEndings = "\r \n \r\n \n \r";

        Serialize(mixedLineEndings)
            .ShouldBe("""
                      "\r \n \r\n \n \r"
                      """);

        var apparentEscapeSequences =
            """
            \u0020
            \u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r
            \u000C\f \u000B\v \u001B\e \u0022\" \u0027\' \u005C\\
            """;

        Serialize(apparentEscapeSequences)
            .ShouldBe($"""
                       "\\u0020{newLineEscaped}\\u0000\\0 \\u0007\\a \\u0008\\b \\u0009\\t \\u000A\\n \\u000D\\r{newLineEscaped}\\u000C\\f \\u000B\\v \\u001B\\e \\u0022\\\u0022 \\u0027\\\u0027 \\u005C\\\\"
                       """);

        var containsApparentOneQuotedRawLiteral =
            """
            "
            Value contains an apparent one-quotes bounded raw string literal.
            "
            """;

        var containsApparentTwoQuotedRawLiteral =
            """
            ""
            Value contains an apparent two-quotes bounded raw string literal.
            ""
            """;

        var containsApparentThreeQuotedRawLiteral =
            """"
            """
            Value contains an apparent three-quotes bounded raw string literal.
            """
            """";

        var containsApparentFourQuotedRawLiteral =
            """""
            """"
            Value contains an apparent four-quotes bounded raw string literal.
            """"
            """"";

        Serialize(containsApparentOneQuotedRawLiteral)
            .ShouldBe($"""
                       "\u0022{newLineEscaped}Value contains an apparent one-quotes bounded raw string literal.{newLineEscaped}\u0022"
                       """);
        
        Serialize(containsApparentTwoQuotedRawLiteral)
            .ShouldBe($"""
                      "\u0022\u0022{newLineEscaped}Value contains an apparent two-quotes bounded raw string literal.{newLineEscaped}\u0022\u0022"
                      """);

        Serialize(containsApparentThreeQuotedRawLiteral)
            .ShouldBe($""""
                       "\u0022\u0022\u0022{newLineEscaped}Value contains an apparent three-quotes bounded raw string literal.{newLineEscaped}\u0022\u0022\u0022"
                       """");

        Serialize(containsApparentFourQuotedRawLiteral)
            .ShouldBe($"""""
                       "\u0022\u0022\u0022\u0022{newLineEscaped}Value contains an apparent four-quotes bounded raw string literal.{newLineEscaped}\u0022\u0022\u0022\u0022"
                       """"");
    }

    public void ShouldSerializeEnums()
    {
        Serialize((StringSplitOptions?)null)
            .ShouldBe("null");

        Serialize(StringSplitOptions.None)
            .ShouldBe("0");

        Serialize(StringSplitOptions.RemoveEmptyEntries)
            .ShouldBe("1");

        Serialize(StringSplitOptions.TrimEntries)
            .ShouldBe("2");

        Serialize((StringSplitOptions)int.MaxValue)
            .ShouldBe("2147483647");
    }

    public void ShouldSerializeGuids()
    {
        Serialize(Guid.Parse("1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"))
            .ShouldBe(
            """
            "1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"
            """);
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

        Serialize(typeof(Utility)).ShouldBe("typeof(Tests.Utility)");
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
    }

    public void ShouldSerializeLists()
    {
        Serialize((int[]?)null)
            .ShouldBe("null");

        Serialize((int[])[])
            .ShouldBe("[]");

        Serialize((int[])[1, 2, 3])
            .ShouldBe("""
                      [
                        1,
                        2,
                        3
                      ]
                      """);

        Serialize((List<string>)["ABC", "123"])
            .ShouldBe("""
                      [
                        "ABC",
                        "123"
                      ]
                      """);
    }

    public void ShouldSerializeObjectProperties()
    {
        Serialize((object?)null)
            .ShouldBe("null");

        Serialize(new Person("Alex", 32))
            .ShouldBe("""
                      {
                        "Name": "Alex",
                        "Age": 32
                      }
                      """);

        Serialize(new PointProperties { X = 1, Y = 2 })
            .ShouldBe("""
                      {
                        "X": 1,
                        "Y": 2
                      }
                      """);

        Serialize(HttpMethod.Post)
            .ShouldBe("""
                      {
                        "Method": "POST"
                      }
                      """);
    }

    public void ShouldSerializeDictionaries()
    {
        Dictionary<string, object>? stringKeys = null;

        Serialize(stringKeys)
            .ShouldBe("null");

        stringKeys = new Dictionary<string, object>
        {
            { "First Key", "First Value" },
            { "Second Key", "Second Value" },
            { "Third Key", "Third Value" }
        };

        Serialize(stringKeys)
            .ShouldBe("""
                      {
                        "First Key": "First Value",
                        "Second Key": "Second Value",
                        "Third Key": "Third Value"
                      }
                      """);

        Dictionary<int, object>? numericKeys = null;

        Serialize(numericKeys)
            .ShouldBe("null");

        numericKeys = new Dictionary<int, object>
        {
            { 1, "First Value" },
            { 2, "Second Value" },
            { 3, "Third Value" }
        };

        Serialize(numericKeys)
            .ShouldBe("""
                      {
                        "1": "First Value",
                        "2": "Second Value",
                        "3": "Third Value"
                      }
                      """);
    }

    public void ShouldSerializingAmbiguouslyUninterestingObjects()
    {
        Serialize(new object())
            .ShouldBe("{}");

        Serialize(new Empty())
            .ShouldBe("{}");

        Serialize(new PointFields { x = 1, y = 2 })
            .ShouldBe("{}");

        Serialize(new Sample("A"))
            .ShouldBe("{}");
            
        Serialize(new SampleNullToString())
            .ShouldBe("{}");
    }

    public void ShouldNotSerializeUnsupportedTypes()
    {
        Action unsupported = () => Serialize(IntPtr.Zero);

        unsupported.ShouldThrow<NotSupportedException>("Serialization and deserialization of 'System.IntPtr' instances is not supported. Path: $.");
    }

    static string Serialize<T>(T value)
        => CsonSerializer.Serialize(value);

    struct Empty;
    
    struct PointFields
    {
        public int x;
        public int y;
        
        public override string ToString() => $"({x},{y})";
    }

    struct PointProperties
    {
        public int X { get; init; }
        public int Y { get; init; }
        
        public override string ToString() => $"({X},{Y})";
    }

    record Person(string Name, int Age);

    class Sample(string name)
    {
        public override string ToString() => $"Sample {name}";
    }

    class SampleNullToString
    {
        public override string? ToString() => null;
    }

    static IEnumerable<char> UnicodeEscapedCharacters()
    {
        // Code points from \u0000 to \u001F, \u007F, and from \u0080 to \u009F are
        // "control characters". Some of these have single-character escape sequences
        // like '\u000A' being equivalent to '\n'. When we omit code points better
        // served by single-character escape sequences, we are left with those deserving
        // '\uHHHH' hex escape sequences.

        for (char c = '\u0001'; c <= '\u0006'; c++) yield return c;
        for (char c = '\u000E'; c <= '\u001F'; c++) yield return c;
        yield return '\u007F';
        for (char c = '\u0080'; c <= '\u009F'; c++) yield return c;

        // Several code points represent various kinds of whitespace. Some of these have
        // single-character escape sequences like '\u0009' being equivalent to '\t', and
        // the single space character ' ' is naturally represented with no need for any
        // escape sequence. All other whitespace code points deserve '\uHHHH' hex escape
        // sequences.

        foreach (char c in (char[])['\u0085', '\u00A0', '\u1680']) yield return c;
        for (char c = '\u2000'; c <= '\u2009'; c++) yield return c;
        foreach (char c in (char[])['\u200A', '\u2028', '\u2029', '\u202F', '\u205F', '\u3000']) yield return c;
    }
}