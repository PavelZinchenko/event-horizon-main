# Expression Parser Issues

This file records problems in the `Assets/ExpressionParser/` submodule and in the way the database calls it. For the design and the language reference, see [Expression Parser](../expression_parser.md). Compile errors that become a silent 0 are already recorded as [DB6](database.md#db6-expression-errors-appear-late-and-become-zero) and are not repeated here.

The analysis was done on 2026-10-12. The runtime sources were compiled outside Unity with a stub for `Mathf` and `Random`, and each formula below was evaluated with `VariantExpressionParser`. Severity is High, Medium, or Low. Status is **Confirmed** (reproduced or the code path is certain), **Confirmed (latent)** (certain, but no built-in formula triggers it), or **Plausible** (depends on data or callers that were not checked).

No formula in the built-in database (`Assets/Modules/Database/Resources/Database/`) triggers EP1, EP2, or EP3. The built-in data uses only `RANDOM(0, 360)` from the functions that the fork added. The risk is in mods.

## Defects

### EP1. `RANDOM_INT` cannot be called

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** The function grammar reads a letter followed by letters or digits ([ExpressionParser.cs:114](../../Assets/ExpressionParser/Runtime/ExpressionParser.cs#L114)). `MakeFunction` has a `RANDOM_INT` case ([line 272](../../Assets/ExpressionParser/Runtime/ExpressionParser.cs#L272)) that the grammar can never produce.

**Failure.**
1. A mod formula uses `RANDOM_INT(1, 5)`.
2. The function rule reads `RANDOM` and fails at `_`. The variable rule then reads `RANDOM_INT` and the parser fails at `(`.
3. `Compile` throws `ExpressionParseException`. The database logs it and the field returns 0 ([DB6](database.md#db6-expression-errors-appear-late-and-become-zero)).

The same rule blocks any external function whose name contains `_`.

**Fix.** Allow `_` after the first character of a function name, the same as for variables. Add a test for `RANDOM_INT`.

### EP2. A literal with `.0` is an integer

**Severity:** Medium. **Status:** Confirmed (latent).

**Where.** `Variant.Parse` tries `int.TryParse` with `NumberStyles.Any` first ([Variant.cs:49](../../Assets/ExpressionParser/Runtime/Variant.cs#L49)). That style accepts `2.0` as the integer 2.

**Failure.** With `level = 3`:

| Formula | Expected | Actual |
| --- | --- | --- |
| `level / 2.0` | 1.5 | 1 |
| `5.0 / 2` | 2.5 | 2 |
| `level / 2.5` | 1.2 | 1.2 |

A modder who writes `.0` to force float division gets integer division. The value is then clamped and used, so nothing reports the error.

**Fix.** Parse a literal that contains `.` as a float. This changes the result of every mod formula that relies on the current behaviour. Announce it to modders and compare the built-in database results before and after.

### EP3. Comparison results have mixed types

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** [VariantExpressionParser.cs:28](../../Assets/ExpressionParser/Runtime/VariantExpressionParser.cs#L28) to line 33. `<`, `<=`, and `!=` return the integer 1 or 0. `>`, `>=`, and `==` return a `bool`. `Variant ==` returns false when the two types differ and neither is a float ([Variant.cs:113](../../Assets/ExpressionParser/Runtime/Variant.cs#L113)).

**Failure.**

| Formula | Expected | Actual |
| --- | --- | --- |
| `(1 < 2) == (2 > 1)` | true | false |
| `(1 < 2) == TRUE` | true | false |
| `1 == TRUE` | true | false |
| `(2 > 1) == TRUE` | true | true |

**Fix.** Return the same type from all six comparisons, and compare `bool` and `int` by value in `==`.

### EP4. Runtime errors escape the expression

**Severity:** Low. **Status:** Confirmed (exception); Plausible (effect on the caller).

**Where.** Integer `/` and `%` use C# integer division. `Variant` negation throws for a `bool`. `AND` and `OR` evaluate both operands ([ExpressionParser.cs:210](../../Assets/ExpressionParser/Runtime/ExpressionParser.cs#L210) and `MakeBinary`). The generated `Evaluate` methods do not catch exceptions ([IntToInt.cs:47](../../Assets/Modules/Database/Scripts/GeneratedGameCode/Expressions/IntToInt.cs#L47)).

**Failure.**
1. A mod formula is `100 / level`, or `level > 0 AND 100 / level > 5`.
2. The game calls it with `level = 0`.
3. `DivideByZeroException` goes up to the game code that called `Evaluate`. `-TRUE` throws `InvalidOperationException` the same way.

The guard in the second formula does not help, because `AND` does not short-circuit. `IF(level > 0, 100 / level, 0)` is safe, because `IF` evaluates only the selected branch.

**Fix.** In the generated expression classes, catch arithmetic exceptions in `Evaluate`, log the formula once, and return the clamped default. Make `AND` and `OR` short-circuit.

### EP5. `SIGN` of a float zero is 1

**Severity:** Low. **Status:** Confirmed (latent).

**Where.** `Variant.Sign` and `FloatExpressionParser.Sign` call `MathCompat.Sign` ([Variant.cs:214](../../Assets/ExpressionParser/Runtime/Variant.cs#L214), [FloatExpressionParser.cs:54](../../Assets/ExpressionParser/Runtime/FloatExpressionParser.cs#L54)). It copies `Mathf.Sign`, so `SIGN(0.0)` is 1. The code comment promises 0.

**Failure.** `SIGN(0.5 - 0.5)` is 1. `SIGN(0)` and `SIGN(0.0)` are 0, but only because both literals are integers ([EP2](#ep2-a-literal-with-0-is-an-integer)).

**Fix.** Use `Math.Sign` for floats too.

### EP6. The README does not match the code

**Severity:** Low. **Status:** Confirmed.

**Where.** [README.md](../../Assets/ExpressionParser/README.md).

**Failure.** The README is the first page a modder finds in the fork. It says:

- Equality is `=`. The parser accepts only `==` ([README.md:59](../../Assets/ExpressionParser/README.md#L59)).
- `AND` and `OR` have the same precedence. `AND` binds tighter.
- The sample uses `ExpresionContext`, which is now an obsolete type that does not compile.

It also lists none of the functions from `SIN` to `RANDOM_INT`.

**Fix.** Update the README in the fork, or replace it with a link to [Expression Parser: Language reference](../expression_parser.md#language-reference).

## Structural issues

### EP7. Each formula builds a new grammar

**Severity:** Low. **Status:** Confirmed.

**Where.** `ExpressionBuilder.Build` creates a new `VariantExpressionParser` for each formula ([ExpressionBuilder.cs:38](../../Assets/Modules/Database/Scripts/Expression/ExpressionBuilder.cs#L38)). The grammar is cached per parser object ([ExpressionParser.cs:32](../../Assets/ExpressionParser/Runtime/ExpressionParser.cs#L32)), so the cache never helps.

**Failure.** The first call of every expression field builds the whole Sprache grammar again. The cost adds up when many fields compile in the same frame. The cost was not measured.

**Fix.** Use the static `VariantExpressionParser.Instance`. The grammar is read-only after it is built, but `_parserCached` is assigned lazily without a lock, so build it once on the main thread.

### EP8. The fork has untested and unused code

**Severity:** Low. **Status:** Confirmed.

**Where.** [Tests/](../../Assets/ExpressionParser/Tests/), [IntExpressionParser.cs](../../Assets/ExpressionParser/Runtime/IntExpressionParser.cs).

**Failure.** `ParserTests` covers the float parser, which the game does not use. `VariantTests` has seven literal and addition cases. No test covers `Variant` division, comparison, external functions, or any function that the fork added. EP1 to EP5 would each fail a one-line test. `IntExpressionParser` has no caller, and ten of its operations throw `NotImplementedException`.

**Fix.** Move the parser tests to `VariantExpressionParser`. Delete `IntExpressionParser`, or implement it.
