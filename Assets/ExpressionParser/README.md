# Expression Parser [![Github license](https://img.shields.io/github/license/codewriter-packages/Expression-Parser.svg?style=flat-square)](#) [![Unity 2020.1](https://img.shields.io/badge/Unity-2020.1+-2296F3.svg?style=flat-square)](#) ![GitHub package.json version](https://img.shields.io/github/package-json/v/codewriter-packages/Expression-Parser?style=flat-square)
_Simple expression parser library for unity_

## Quick Sample

```csharp
// create context
var context = new ExpresionContext<float>();

// register some variables
context.RegisterVariable("a", () => 1);
context.RegisterVariable("b", () => 2);
context.RegisterVariable("c", () => 3);

var input = "a >= b AND NOT(b) OR (a + b) >= c";

// compile expression
var compiledExpr = FloatExpressionParser.Instance.Compile(input, context, true);

// execute expression
var result = compiledExpr.Invoke();

```

Create your own `ExpressionParser` implementation to support other data types

## Language Overview

#### Constants

| Input | Value |
| ----- | ----- |
| TRUE  | 1     |
| FALSE | 0     |

#### Numbers

| Input | Value |
| ----- | ----- |
| 123   | 123.0 |
| 123.0 | 123.0 |
| 123.4 | 123.4 |

#### Operators

| Name        | Sample | Precedence |
| ----------- | ------ | ---------- |
| Unary Minus | -2     | 0  |
| Power       | 2 ^ 3  | 1 (!!!, -2^2 = (-2) ^ 2 = 4)  |
| Multiply    | 2 * 3  | 2  |
| Divide      | 6 / 3  | 2  |
| Module      | 5 % 3  | 2  |
| Addition    | 2 + 3  | 3  |
| Subtraction | 3 - 2  | 3  |
| Less Than   | 2 < 3  | 4  |
| Less Than Or Equal | 2 <= 3 | 4 |
| Greater Than | 2 > 3  | 4  |
| Greater Than Or Equal | 2 >= 3 | 4 |
| Equality    | 2 = 3  | 5  |
| Inequality  | 2 != 3 | 5  |
| Logical and | 2 > 1 AND 3 > 2 | 6 |
| Logical or | 2 = 3 OR 2 != 4 | 6 |

#### Variables

Creation of custom variables is supported.
Variable name must starts with english letter or underscore.
The rest of the variable name may also contains digits.

#### Functions

| Name | Sample    | Description |
| ---- | --------- | ----------- |
| NOT  | NOT(TRUE) | Returns FALSE if argument is true, otherwise TRUE |
| IF   | IF(n < 5, 5, n < 25, 25, 50) | Returns the second argument if the first argument is true, returns the fourth argument if the third argument is true, and so on. Otherwise, returns the last argument. Odd number of arguments required   |
| MIN  | MIN(0, 1, ...) | Returns smallest argument |
| MAX  | MAX(0, 1, ...) | Returns largest argument |
| ROUND   | ROUND(1.1) | Rounds a number |
| CEILING | CEILING(1.1) | Round number up |
| FLOOR   | FLOOR(1.1) | Rounds number down |
| LOG | LOG(8, 2) | Returns the logarithm to base B of number A |

## How to Install
Minimal Unity Version is 2020.1.

Library distributed as git package ([How to install package from git URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html))
<br>Git URL: `https://github.com/codewriter-packages/Expression-Parser.git`

## License

Expression-Parser is [MIT licensed](./LICENSE.md).
