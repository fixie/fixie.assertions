using static System.Environment;
using static Fixie.Assertions.CsonSerializer;

namespace Tests;

class CsonTextTests
{
    public void ShouldSerializeChars()
    {
        Serialize('a')
            .ShouldBe("""
                      'a'
                      """);

        Serialize('☺')
            .ShouldBe("""
                      '☺'
                      """);
        
        // Escape Sequence: Null
        Serialize('\u0000')
            .ShouldBe("""
                      '\0'
                      """);
        Serialize('\0')
            .ShouldBe("""
                      '\0'
                      """);

        // Escape Sequence: Alert
        Serialize('\u0007')
            .ShouldBe("""
                      '\a'
                      """);
        Serialize('\a')
            .ShouldBe("""
                      '\a'
                      """);

        // Escape Sequence: Backspace
        Serialize('\u0008')
            .ShouldBe("""
                      '\b'
                      """);
        Serialize('\b')
            .ShouldBe("""
                      '\b'
                      """);

        // Escape Sequence: Horizontal tab
        Serialize('\u0009')
            .ShouldBe("""
                      '\t'
                      """);
        Serialize('\t')
            .ShouldBe("""
                      '\t'
                      """);

        // Escape Sequence: New line
        Serialize('\u000A')
            .ShouldBe("""
                      '\n'
                      """);
        Serialize('\n')
            .ShouldBe("""
                      '\n'
                      """);

        // Escape Sequence: Vertical tab
        Serialize('\u000B')
            .ShouldBe("""
                      '\v'
                      """);
        Serialize('\v')
            .ShouldBe("""
                      '\v'
                      """);

        // Escape Sequence: Form feed
        Serialize('\u000C')
            .ShouldBe("""
                      '\f'
                      """);
        Serialize('\f')
            .ShouldBe("""
                      '\f'
                      """);

        // Escape Sequence: Carriage return
        Serialize('\u000D')
            .ShouldBe("""
                      '\r'
                      """);
        Serialize('\r')
            .ShouldBe("""
                      '\r'
                      """);

        // TODO: Applicable in C# 13
        // Escape Sequence: Escape
        // Serialize('\u001B')
        //     .ShouldBe("""
        //               '\e'
        //               """);
        // Serialize('\e')
        //     .ShouldBe("""
        //               '\e'
        //               """);

        // Literal Space
        Serialize('\u0020')
            .ShouldBe("""
                      ' '
                      """);
        Serialize(' ')
            .ShouldBe("""
                      ' '
                      """);

        // Escape Sequence: Double quote
        Serialize('\u0022')
            .ShouldBe("""
                      '\"'
                      """);
        Serialize('\"')
            .ShouldBe("""
                      '\"'
                      """);

        // Escape Sequence: Single quote
        Serialize('\u0027')
            .ShouldBe("""
                      '\''
                      """);
        Serialize('\'')
            .ShouldBe("""
                      '\''
                      """);

        // Escape Sequence: Backslash
        Serialize('\u005C')
            .ShouldBe("""
                      '\\'
                      """);
        Serialize('\\')
            .ShouldBe("""
                      '\\'
                      """);

        foreach (var c in UnicodeEscapedCharacters())
            Serialize(c)
                .ShouldBe($"""
                           '\u{(int)c:X4}'
                           """);

        Serialize((char?)null)
            .ShouldBe("null");
        
        Serialize((char?)'a')
            .ShouldBe("""
                      'a'
                      """);
    }

    public void ShouldSerializeStrings()
    {
        Serialize((string?)null)
            .ShouldBe("null");

        Serialize("a☺")
            .ShouldBe("""
                      "a☺"
                      """);

        Serialize("\u0020 ")
            .ShouldBe("""
                      "  "
                      """);

        Serialize("\u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r")
            .ShouldBe("""
                      "\0\0 \a\a \b\b \t\t \n\n \r\r"
                      """);

        // TODO: In C# 13, include \u001B\e becoming \e\e
        Serialize("\u000C\f \u000B\v \u0022\" \u0027\' \u005C\\")
            .ShouldBe("""
                      "\f\f \v\v \"\" \'\' \\\\"
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
            .ShouldBe(""""
                      """
                      Line 1
                      Line 2
                      Line 3
                      Line 4
                      """
                      """");

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
            .ShouldBe($""""
                       """
                       \u0020
                       \u0000\0 \u0007\a \u0008\b \u0009\t \u000A\n \u000D\r
                       \u000C\f \u000B\v \u001B\e \u0022\" \u0027\' \u005C\\
                       """
                       """");

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
            .ShouldBe(""""
                      """
                      "
                      Value contains an apparent one-quotes bounded raw string literal.
                      "
                      """
                      """");
        
        Serialize(containsApparentTwoQuotedRawLiteral)
            .ShouldBe(
                      """"
                      """
                      ""
                      Value contains an apparent two-quotes bounded raw string literal.
                      ""
                      """
                      """");

        Serialize(containsApparentThreeQuotedRawLiteral)
            .ShouldBe("""""
                      """"
                      """
                      Value contains an apparent three-quotes bounded raw string literal.
                      """
                      """"
                      """"");

        Serialize(containsApparentFourQuotedRawLiteral)
            .ShouldBe(""""""
                      """""
                      """"
                      Value contains an apparent four-quotes bounded raw string literal.
                      """"
                      """""
                      """""");
    }

    public void ShouldSerializeMultilineStringsRespectingIndentation()
    {
        Model[] items =
        [
            new()
            {
                Text = "ABC",
                Inner = new() {
                    Text = "DEF"
                },
                Pairs = new() {
                    ["ABC"] = "DEF"
                },
                List = [
                    "GHI",
                    "JKL"
                ]
            },
            new()
            {
                Text = """
                       Line 1
                       Line 2
                       """,
                Inner = new() {
                    Text = """
                           
                           Line 2
                           Line 3
                           """
                },
                Pairs = new() {
                    ["ABC"] = """
                              Line 1
                              Line 2
                              """,
                    ["DEF"] = """
                              Line 1
                              
                              Line 3
                              """,
                    ["""
                     Line 1
                     Line 2
                     """] = """
                            Line 1
                            Line 2
                            
                            """
                },
                List = [
                    """
                    Line 1
                    Line 2
                    Line 3
                    """,
                    "ABC",
                    """
                    Line 1
                    Line 2
                    Line 3
                    """
                ]
            }
        ];

        Serialize(items)
            .ShouldBe(
            """"
            [
              {
                Text = "ABC",
                Inner = {
                  Text = "DEF"
                },
                Pairs = {
                  ["ABC"] = "DEF"
                },
                List = [
                  "GHI",
                  "JKL"
                ]
              },
              {
                Text = """
                       Line 1
                       Line 2
                       """,
                Inner = {
                  Text = """
                         
                         Line 2
                         Line 3
                         """
                },
                Pairs = {
                  ["ABC"] = """
                            Line 1
                            Line 2
                            """,
                  ["DEF"] = """
                            Line 1
                            
                            Line 3
                            """,
                  ["""
                   Line 1
                   Line 2
                   """] = """
                          Line 1
                          Line 2
                          
                          """
                },
                List = [
                  """
                  Line 1
                  Line 2
                  Line 3
                  """,
                  "ABC",
                  """
                  Line 1
                  Line 2
                  Line 3
                  """
                ]
              }
            ]
            """");
    }

    class Model
    {
        public required string Text { get; init; }
        public required InnerModel Inner { get; init; }
        public required Dictionary<string, string> Pairs { get; init; }
        public required string[] List { get; init; }
    }

    class InnerModel
    {
        public required string Text { get; init; }
    }

    public void ShouldSerializeGuids()
    {
        Serialize(Guid.Parse("1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"))
            .ShouldBe(
            """
            "1f39a64c-cb96-4f1f-8b0f-ab8f6d153a7e"
            """);

        Serialize((Guid?)null)
            .ShouldBe("null");

        Serialize((Guid?)Guid.Parse("2fc7251f-a8d0-4573-87a5-d12408231e77"))
            .ShouldBe(
            """
            "2fc7251f-a8d0-4573-87a5-d12408231e77"
            """);
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