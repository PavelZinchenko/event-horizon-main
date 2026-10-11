# Expression Parser

`Assets/ExpressionParser/` is a Git submodule with a fork of the CodeWriter Expression Parser library. It compiles a text formula, for example `MIN(1000, 300 + 5*distance)`, into a C# delegate. The game uses it only for database expression fields: mods and the built-in database store formulas as strings, and the [Database module](database.md#expressions) compiles them on first use.

| | Value |
| --- | --- |
| Assembly | [CodeWriter.ExpressionParser.asmdef](../Assets/ExpressionParser/Runtime/CodeWriter.ExpressionParser.asmdef): no references, every platform, `autoReferenced` |
| Test assembly | [CodeWriter.ExpressionParser.Tests.asmdef](../Assets/ExpressionParser/Tests/CodeWriter.ExpressionParser.Tests.asmdef): editor only, NUnit |
| Namespaces | `CodeWriter.ExpressionParser`; `Sprache` (vendored parser combinators) |
| Source | [Runtime/](../Assets/ExpressionParser/Runtime/): 4 parser files and `Variant`, about 930 lines; `Sprache/`, 22 files, about 2,260 lines |
| Version | `package.json` says 1.0.1. The code is newer: upstream added `LOG` later, and the fork added the `int` and `Variant` parsers, external functions, and 11 more functions. |
| Upstream | `codewriter-packages/Expression-Parser`, MIT, © 2021 Vlad Vanifatov. Sprache: MIT, © 2011 Nicholas Blumhardt. |
| Fork | `PavelZinchenko/Expression-Parser`, last commit 2024-03-21 |
| Consumers | `Database` only ([ExpressionBuilder.cs](../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs) and the generated `GameDatabase.Expressions.*` classes) |
| Known issues | [Expression Parser issues](issues/expression_parser.md) |

## Contents

1. [Terms](#terms)
2. [Source map](#source-map)
3. [Compile pipeline](#compile-pipeline)
4. [Language reference](#language-reference)
5. [Variant values](#variant-values)
6. [How the database uses the parser](#how-the-database-uses-the-parser)
7. [Tests](#tests)
8. [Working on the parser](#working-on-the-parser)

## Terms

| Term | Meaning |
| --- | --- |
| Formula | The source text of an expression, as stored in a database JSON field. |
| Compiled expression | An `Expression<T>` delegate (`T Expression<out T>()`). Each call evaluates the formula with the current variable values. |
| Context | An `ExpressionContext<T>`. It resolves variable and external function names. A context can have a parent context. |
| Builder | An internal `ExprBuilder` delegate. Parsing produces a builder tree. Invoking the builder with a context binds the names and returns the compiled expression. |
| External function | A function that the context supplies through `IFunction<T>`, not one of the built-in names. The database uses external functions for expression fields of the same record. |
| `Variant` | A value that is an `int`, a `float`, or a `bool`. The game parser works on `Variant`. |

## Source map

| File | Role |
| --- | --- |
| [ExpressionParser.cs](../Assets/ExpressionParser/Runtime/ExpressionParser.cs) | `ExpressionParser<T>`: the grammar, the built-in functions, and the abstract arithmetic. Also `ExpressionContext<T>`, `IFunction<T>`, and the three exception types. |
| [VariantExpressionParser.cs](../Assets/ExpressionParser/Runtime/VariantExpressionParser.cs) | `ExpressionParser<Variant>`. The only implementation that the game uses. |
| [Variant.cs](../Assets/ExpressionParser/Runtime/Variant.cs) | The `Variant` struct: literal parsing, arithmetic, comparison, and math helpers. |
| [FloatExpressionParser.cs](../Assets/ExpressionParser/Runtime/FloatExpressionParser.cs) | `ExpressionParser<float>`. Used only by the tests. |
| [IntExpressionParser.cs](../Assets/ExpressionParser/Runtime/IntExpressionParser.cs) | `ExpressionParser<int>`. No callers. `^`, `LOG`, the trigonometric functions, `RAD2DEG`, and `DEG2RAD` throw `NotImplementedException`. |
| [Sprache/](../Assets/ExpressionParser/Runtime/Sprache/) | A copy of the Sprache parser-combinator library. |
| [Tests/](../Assets/ExpressionParser/Tests/) | `ParserTests` (float parser) and `VariantTests`. |
| [README.md](../Assets/ExpressionParser/README.md) | The upstream README. Parts of it are out of date ([EP6](issues/expression_parser.md#ep6-the-readme-does-not-match-the-code)). Use this page instead. |

## Compile pipeline

`Compile(input, context, cache)` works in three steps:

1. The parser object builds the grammar once and keeps it in `_parserCached`.
2. The grammar parses the text into a builder tree. With `cache = true`, the parser object also keeps the builder for each input string. A parse failure throws `ExpressionParseException`.
3. The builder tree is invoked with the context. Each name is resolved at this point. An unknown variable throws `VariableNotDefinedException`. An unknown function, or a wrong argument count, throws `FunctionNotDefinedException`.

The result is a delegate tree. Evaluation does not parse or look up names again; it only calls the bound delegates.

`CompilePredicate` wraps the result in a `bool` delegate. The game does not call it.

## Language reference

This section describes the code, not the upstream README.

### Literals and names

| Item | Rule |
| --- | --- |
| Number | Digits with an optional decimal part, for example `12`, `0.25`, or `.5`. No exponent: `1e3` fails. The invariant culture is used. For the type of a literal, see [Variant values](#variant-values). |
| `TRUE`, `FALSE` | Boolean constants. Upper case only. |
| Variable | Starts with a letter or `_`, then letters, digits, or `_`. Case-sensitive. |
| Function call | `NAME(arg, ...)`. The name is a letter followed by letters or digits. An underscore is not allowed in a function name ([EP1](issues/expression_parser.md#ep1-random_int-cannot-be-called)). Built-in names are upper case and case-sensitive. |
| Whitespace | Allowed around operators and operands. |

### Operators

From the highest precedence to the lowest:

| Level | Operators | Grouping | Notes |
| --- | --- | --- | --- |
| 1 | unary `-` | prefix | Applies to one operand, so `-2^2` is `(-2)^2 = 4`. `--2` does not parse. |
| 2 | `^` | right to left | Power. |
| 3 | `*`, `/`, `%` | left to right | `/` and `%` on two integers are integer operations. |
| 4 | `+`, `-` | left to right | |
| 5 | `<`, `<=`, `>`, `>=` | none | `1 < 2 < 3` does not parse. |
| 6 | `==`, `!=` | none | `=` does not parse. |
| 7 | `AND` | left to right | Binds tighter than `OR`. |
| 8 | `OR` | left to right | |

`AND` and `OR` return one of their operands, not a boolean: `a AND b` returns `b` when `a` is true, otherwise `a`. `a OR b` returns `a` when `a` is true, otherwise `b`. Both operands are always evaluated. There is no short-circuit ([EP4](issues/expression_parser.md#ep4-runtime-errors-escape-the-expression)).

A value is true when it is not zero.

### Built-in functions

| Function | Arguments | Result |
| --- | --- | --- |
| `NOT(a)` | 1 | `FALSE` when `a` is true, otherwise `TRUE`. |
| `IF(c1, r1, c2, r2, ..., else)` | odd, at least 3 | The first `r` whose `c` is true, otherwise `else`. Only the selected branch is evaluated. |
| `MIN(a, ...)`, `MAX(a, ...)` | at least 1 | The smallest or largest argument, with its own type. |
| `ROUND(a)`, `CEILING(a)`, `FLOOR(a)` | 1 | An integer. `ROUND` rounds a half to the even number (`ROUND(2.5) = 2`). |
| `ABS(a)`, `SIGN(a)` | 1 | Keeps the integer or float type. `SIGN` of a float zero is `1` ([EP5](issues/expression_parser.md#ep5-sign-of-a-float-zero-is-1)). |
| `LOG(a)` | 1 | Base-10 logarithm, float. |
| `LOG(a, b)` | 2 | Logarithm of `a` to base `b`, float. |
| `SIN`, `COS`, `TAN`, `ATAN` | 1 | Float, radians. |
| `ATAN2(y, x)` | 2 | Float, radians, the angle of the point (`x`, `y`). The code names the parameters `x, y`, but passes them to `Mathf.Atan2(y, x)` in that order. |
| `RAD2DEG(a)`, `DEG2RAD(a)` | 1 | Float. |
| `RANDOM(a, b)` | 2 | Float from `UnityEngine.Random.Range(a, b)`. Both ends are included. |
| `RANDOM_INT(a, b)` | 2 | Declared, but cannot be called ([EP1](issues/expression_parser.md#ep1-random_int-cannot-be-called)). |

Any other name followed by `(` is an external function. The context resolves it when the formula is bound.

`RANDOM` uses the shared `UnityEngine.Random` state. It does not use the game's seeded random generators. A formula that uses `RANDOM` therefore gives a different value on each call and changes the sequence of every other `UnityEngine.Random` user.

## Variant values

`Variant` has three types: `int`, `float` (single precision), and `bool`.

### Literal types

`Variant.Parse` tries `int` first with `NumberStyles.Any`. That style accepts a decimal point when the fraction is zero. So `2.0` is the integer `2`, and `2.5` is the float `2.5` ([EP2](issues/expression_parser.md#ep2-a-literal-with-0-is-an-integer)).

### Arithmetic

| Operation | Two integers | Otherwise |
| --- | --- | --- |
| `+`, `-`, `*` | integer | float |
| `/` | integer division, truncated toward zero; divide by zero throws | float; divide by zero gives infinity or NaN |
| `%` | integer remainder; zero throws | float remainder |
| `^` | integer when both are integers and the result is within ±16,777,217; otherwise float. `2^-1` is `0`. | float |
| unary `-` | integer | float. A `bool` operand throws `InvalidOperationException`. |

A `bool` takes part in arithmetic as 0 or 1 (`TRUE + TRUE = 2`).

### Comparison

| Operator | Result type |
| --- | --- |
| `<`, `<=`, `!=` | integer 0 or 1 |
| `>`, `>=`, `==` | `bool` |

`==` with a float on either side uses `Mathf.Approximately`. Otherwise it is true only when both sides have the same type and the same integer value. So `1 == TRUE` is false, and `(1 < 2) == (2 > 1)` is false ([EP3](issues/expression_parser.md#ep3-comparison-results-have-mixed-types)).

### Conversion out of a `Variant`

| Property | Rule |
| --- | --- |
| `AsInt` | A float is truncated toward zero. |
| `AsSingle` | An integer or `bool` is converted. |
| `AsBool` | True when the stored integer or float is not zero. |

## How the database uses the parser

The database bridge is documented in [Database: Expressions](database.md#expressions). This section adds the parser side.

1. The code generator emits one class per expression signature, for example [`IntToInt`](../Assets/Modules/Database/Scripts/GeneratedGameCode/Expressions/IntToInt.cs). Each class implements `IFunction<Variant>`, so one expression field can call another as a function.
2. On the first call, the class creates an [`ExpressionBuilder`](../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs). The builder makes a context with the record's `IVariableResolver` and registers the parameter (for example `level`) in it.
3. `ExpressionBuilder.Build` creates a new `VariantExpressionParser` and compiles with `cache = false` ([EP7](issues/expression_parser.md#ep7-each-formula-builds-a-new-grammar)).
4. A parse or bind error is logged and the formula becomes a function that returns 0 ([DB6](issues/database.md#db6-expression-errors-appear-late-and-become-zero)).
5. Each call stores the argument in a field, evaluates the delegate, converts with `AsInt` or `AsSingle`, and clamps the result to the schema range.

Name resolution order inside a formula:

1. The parameter, registered in the context.
2. The record resolver: other fields of the record, and enum value names of the parameter type.
3. The parent context. The database does not set one.

Function resolution: the built-in names first, then the record resolver. A record field cannot replace a built-in function.

Evaluation errors are not caught. For example, `100 / level` with `level = 0` throws `DivideByZeroException` to the caller of `Evaluate` ([EP4](issues/expression_parser.md#ep4-runtime-errors-escape-the-expression)).

## Tests

The tests run in the Unity Test Runner (Edit Mode). `ParserTests` covers the float parser: operators, precedence, functions, variables, and errors. `VariantTests` has seven cases for `Variant` literals and addition. No test covers `Variant` division or comparison, the functions that the fork added, or external functions.

## Working on the parser

### Adding a built-in function

1. Add an abstract or virtual method to `ExpressionParser<T>`, or a private helper if the base class can implement it.
2. Add a `case` to `MakeFunction`. Use `MakeFunction1`, `MakeFunction2`, or `MakeFunctionFold` for the argument check.
3. Implement the method in `VariantExpressionParser`, `FloatExpressionParser`, and `IntExpressionParser`.
4. Use a name of letters and digits only, because the grammar does not accept `_` in a function name.
5. Add a test for the `Variant` parser.
6. Update [Database: Expressions](database.md#expressions) and the language reference on this page.

A new built-in name hides any record field with the same name. Check the database schema for a collision first.

### Committing a change

The folder is a submodule. Commit and push inside `Assets/ExpressionParser/` first, then commit the new submodule pointer in the main repository.

### Changing number or comparison rules

A change to `Variant` changes the result of existing formulas in the built-in database and in mods. Compare the results of all database expressions before and after the change.
