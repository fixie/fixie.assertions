# Fixie.Assertions

## Ergonomic Assertions for .NET

Fixie.Assertions is a small assertion library. It is easy to understand and trivially open to extension. It produces highly readable error messages.

Assertion libraries are orthogonal to test frameworks. You don't have to use Fixie if you use Fixie.Assertions, and you don't have to use Fixie.Assertions if you use Fixie. Still, they make for a great combination as they both stand on the same design premise:

> Developer ergonomics result from having a small and simple core that can be trivially extended by the user with idiomatic code.

Like other .NET assertion libraries, error messages include context from the original assertion line, including the target expression found on the left of the assertion `.` operator. Unlike other libraries, though, this does not require injecting MSBuild properties into your test project, or stack trace walking, or a Release vs Debug build configuration preference, or tricky parsing of arbitrarily complex C# code. Your test failure messages are never tossed into a blender when things go wrong. Instead, the magic here is provided by the compiler with its inherent awareness of C# syntax using features introduced in .NET 8.

## Equality with `ShouldBe`

```cs
age.ShouldBe(65);
```

```
age should be 65 but was 30
```

```cs
markdownDocument.ToString().ShouldBe(
    """
    # Heading
    
    ## Subheading
    
    Paragraph including *emphasis*.
    """
    );
```

```
markdownDocument.ToString() should be

    """
    # Heading
    
    ## Subheading
    
    Paragraph including *emphasis*.
    """

but was

    """
    # Heading
    
    ## Subheading
    
    Paragraph including **typo**.
    """
```


## Structural Equality with `ShouldMatch`

Other assertion libraries tend to treat array equality checks with structural equality semantics. Doing so complicates the understanding of equality-asserting method names, reduces the ability to say clearly what you mean, and overcomplicates matters in those situations where you meaningfully care whether *this* array is literally *that* array.

With Fixie.Assertions, `ShouldBe` uses idiomatic .NET equality semantics, full stop. When you instead want structural equality, you say so clearly with `ShouldMatch`.

```cs
decimal[] prices = [1.20m, 5.99m, 10.14m];
prices.ShouldBe(prices); // Succeeds.
prices.ShouldBe([1.20m, 5.99m, 10.14m]); // Fails! .NET arrays do not overload `==`.
```

```
prices should be

    [
        1.20,
        5.99,
        10.14
    ]

but was

    [
        1.20,
        5.99,
        10.14
    ]

These serialized values are identical. Did you mean to perform a structural comparison with `ShouldMatch` instead?
```

```cs
decimal[] prices = [1.20m, 5.99m, 10.14m];
prices.ShouldMatch(prices); // Succeeds.
prices.ShouldMatch([1.20m, 5.99m, 10.14m]); // Succeeds by structural comparison.
prices.ShouldMatch([1.20m, 5.99m, 10.14m, 7.34m]); // Fails by structural comparison.
```

```
prices should be

    [
        1.20,
        5.99,
        10.14,
        7.34
    ]

but was

    [
        1.20,
        5.99,
        10.14
    ]
```

`ShouldMatch` will perform a deep comparison of public object state, even against anonymous-typed expectations:

```
myComplexObject.ShouldMatch(new {
    Property = "ABC",
    Field = 123,
    List = [1, 2, 3]
    Dictionary = new Dictionary<string, int> {
        ["A"] = 1,
        ["B"] = 2
    }
    Nested = new {
        Property = "DEF"
    }
});
```

> WARNING: Beware making `ShouldMatch` comparisons between types that have equivalent public structure but meaningfully-different state. You may fool yourself into thinking two objects are equivalent when you would in fact disagree. It is best to witness the textual representation of your type as seen when the assertion fails, as part of a typical "Red, Then Green" implementation of your test, when deciding whether structural comparison is appropriate for the types in question. As a reasonable heuristic, if you would feel unsafe serializing the two objects to JSON and asserting the resulting strings are equal, you should feel unsafe calling `ShouldMatch` for the same reason. As with JSON serialization, extremely nested objects or those with cycles are unsupported and will fail with an explanation.


## Type Pattern Assertions

```cs
object o = "ABC";

o.ShouldBe<string>(); // Succeeds.
o.ShouldBe<int>(); // Fails.
```

```
o should match the type pattern

    is int

but was

    string
```

## Nulls

The `ShouldNotBeNull` assertion provides additional evidence to the compiler as it traces the flow of nullability through your test code. After calling it, the target object is understood to definitely not be null in subsequent statements.

```cs
possiblyNull.Property.ShouldBe(7);
//          ^  Nullability warning here!
```

```cs
possiblyNull.ShouldNotBeNull();
possiblyNull.Property.ShouldBe(7); // No warning here.
```

Upon success, it returns the value unchanged with awareness that it is not null:

```cs
possiblyNull.ShouldNotBeNull().Property.ShouldBe(7); // No warning here.
```

## Expecting Exceptions

```cs
Action divideByZero = () => OperationThatDividesByZero();

divideByZero.ShouldThrow<DivideByZeroException>(); //Allow any message.
divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero");
```

If your operation fails to throw at all:

```
divideByZero should have thrown System.DivideByZeroException but did not.
```

If your operation throws the right exception type, but with the wrong message:

```
divideByZero should have thrown System.DivideByZeroException with message

    "Divided By Zero"

but instead the message was

    "Attempted to divide by zero."
```

If your operation throws the wrong exception type, and you do not specify a message:

```
divideByZero should have thrown System.DivideByZeroException

but instead it threw System.ArgumentNullException with message

    "Value cannot be null. (Parameter \'divisor\')"
```

If your operation throws the wrong exception type, and you do specify a message:

```
divideByZero should have thrown System.DivideByZeroException with message

    "Attempted to divide by zero."

but instead it threw System.ArgumentNullException with message

    "Value cannot be null. (Parameter \'divisor\')"
```

`ShouldThrow` is overloaded for `async`/`await` scenarios, where the operation under test is itself `async`:

```cs
Func<Task> divideByZero = async () => await OperationThatDividesByZero();

await divideByZero.ShouldThrow<DivideByZeroException>("Divided By Zero");

```

## `ShouldSatisfy`

Most assertion libraries tend to accrete 1000 `ShouldXyz` methods for every conceivable situation. This library refuses to boil the ocean. The `ShouldSatisfy(expectation)` assertion reduces the need for things like `ShouldBeGreaterThan`, `ShouldBeGreaterThanOrEqualTo`, and similar nonidiomatic assertions.

```cs
var value = 4;
value.ShouldSatisfy(x => x > 4);
```

```
value should satisfy

    > 4

but was

    4
```

```cs
var value = 3;
value.ShouldSatisfy(x => x >= 4);
```

```
value should satisfy

    >= 4

but was

    3
```

## Integration with Fixie

The properties on `AssertException` are a natural fit for display in your diff tool. When a single test fails, the following custom Fixie report will display the Expected/Actual values in your diff tool.

```xml
<ItemGroup>
  <PackageReference Include="Fixie.TestAdapter" />
  <PackageReference Include="Fixie.Assertions" />
  <PackageReference Include="DiffEngine" />
</ItemGroup>
```

```cs
using Fixie;

namespace Example.Tests;

class TestProject : ITestProject
{
    public void Configure(TestConfiguration configuration, TestEnvironment environment)
    {
        if (environment.IsDevelopment())
            configuration.Reports.Add<DiffToolReport>();
    }
}
```

```cs
using Fixie.Reports;
using Fixie.Assertions;
using DiffEngine;

namespace Example.Tests;

class DiffToolReport : IHandler<TestFailed>, IHandler<ExecutionCompleted>
{
    int failures;
    Exception? singleFailure;

    public Task Handle(TestFailed message)
    {
        failures++;

        singleFailure = failures == 1 ? message.Reason : null;

        return Task.CompletedTask;
    }

    public async Task Handle(ExecutionCompleted message)
    {
        if (singleFailure is AssertException exception)
            if (exception.HasMultilineRepresentation)
                await LaunchDiffTool(exception);
    }

    static async Task LaunchDiffTool(AssertException exception)
    {
        var tempPath = Path.GetTempPath();
        var expectedPath = Path.Combine(tempPath, "expected.txt");
        var actualPath = Path.Combine(tempPath, "actual.txt");

        File.WriteAllText(expectedPath, exception.Expected);
        File.WriteAllText(actualPath, exception.Actual);

        await DiffRunner.LaunchAsync(expectedPath, actualPath);
    }
}
```

Your diff tool launches on failure, drawing attention to the meaningful differences between the expected and actual values:

```diff
[
    1.20,
    5.99,
+    10.14
-    10.14,
-    7.34
]
```
