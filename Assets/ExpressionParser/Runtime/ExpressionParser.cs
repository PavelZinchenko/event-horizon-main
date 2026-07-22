using System;
using System.Collections.Generic;
using System.Linq;
using Sprache;
using static Sprache.Parse;

namespace CodeWriter.ExpressionParser
{
    public delegate T Expression<out T>();

    public interface IFunction<T>
    {
        int ArgumentCount { get; }
        Expression<T> Invoke(List<Expression<T>> arguments);
    }

    public abstract class ExpressionParser<T>
    {
        private readonly Dictionary<string, ExprBuilder> _builderCached = new Dictionary<string, ExprBuilder>();
        private Parser<ExprBuilder> _parserCached;

        public Expression<bool> CompilePredicate(string input, ExpressionContext<T> context, bool cache)
        {
            var expr = Compile(input, context, cache);
            return () => IsTrue(expr.Invoke());
        }

        public Expression<T> Compile(string input, ExpressionContext<T> context, bool cache)
        {
            context = context ?? new ExpressionContext<T>();

            _parserCached = _parserCached ?? CreateParser();

            ExprBuilder builder;
            try
            {
                if (cache)
                {
                    if (!_builderCached.TryGetValue(input, out builder))
                    {
                        builder = _parserCached.Parse(input);
                        _builderCached.Add(input, builder);
                    }
                }
                else
                {
                    builder = _parserCached.Parse(input);
                }
            }
            catch (ParseException parseException)
            {
                throw new ExpressionParseException(input, parseException);
            }

            return builder.Invoke(context);
        }

        private static Parser<BinaryFunc> Operator(string op, BinaryFunc fun)
        {
            return String(op).Token().Return(fun).Named(op);
        }

        private static Parser<ExprBuilder> BinaryOperator(Parser<BinaryFunc> op,
            Parser<ExprBuilder> operand, Func<BinaryFunc, ExprBuilder, ExprBuilder, ExprBuilder> apply) =>
        (
            from l in operand
            from rest in
            (
                from f in op
                from r in operand
                select apply(f, l, r)
            ).Optional()
            select rest.IsEmpty ? l : rest.Get()
        );

        private Parser<ExprBuilder> CreateParser()
        {
            var letterOrUnderscore = Char(c => char.IsLetter(c) || c == '_',
                "letter or underscore");
            var letterOrDigitOrUnderscore = Char(c => char.IsLetterOrDigit(c) || c == '_',
                "letter or digit or underscore");

            var constant = (
                from number in DecimalInvariant
                select MakeConstant(number, Parse)
            ).Named("number");

            var add = Operator("+", Add);
            var sub = Operator("-", Sub);
            var mul = Operator("*", Mul);
            var div = Operator("/", Div);
            var mod = Operator("%", Mod);
            var pow = Operator("^", Pow);
            var eq = Operator("==", Equal);
            var neq = Operator("!=", NotEqual);
            var and = Operator("AND", And);
            var or = Operator("OR", Or);
            var lt = Operator("<", LessThan);
            var lte = Operator("<=", LessThanOrEqual);
            var gt = Operator(">", GreaterThan);
            var gte = Operator(">=", GreaterThanOrEqual);

            var variable =
            (
                from nameHead in letterOrUnderscore.Once().Text()
                from nameTail in letterOrDigitOrUnderscore.Many().Text()
                select MakeVariable(nameHead + nameTail)
            ).Named("variable");

            Parser<ExprBuilder> expression = null;

            var function =
            (
                from nameHead in Letter.Once().Text()
                from nameTail in LetterOrDigit.Many().Text()
                from lparen in Char('(')
                // ReSharper disable once AccessToModifiedClosure
                from expr in Ref(() => expression).DelimitedBy(Char(',').Token())
                from rparen in Char(')')
                select MakeFunction(nameHead + nameTail, expr.ToList())
            ).Named("function");

            var factor =
                (
                    from lparen in Char('(')
                    // ReSharper disable once AccessToModifiedClosure
                    from expr in Ref(() => expression)
                    from rparen in Char(')')
                    select expr
                ).Named("expression")
                .XOr(constant)
                .XOr(function)
                .Or(variable);

            var operand =
            (
                (
                    from sign in Char('-')
                    from fact in factor
                    select MakeUnary(Negate, fact)
                )
                .XOr(factor)
            ).Token();

            var term1 = ChainRightOperator(pow, operand, MakeBinary);
            var term2 = ChainOperator(mul.Or(div).Or(mod), term1, MakeBinary);
            var term3 = ChainOperator(add.Or(sub), term2, MakeBinary);
            var term4 = BinaryOperator(lte.Or(lt).Or(gte).Or(gt), term3, MakeBinary);
            var term5 = BinaryOperator(eq.Or(neq), term4, MakeBinary);
            var term6 = ChainOperator(and, term5, MakeBinary);
            var term7 = ChainOperator(or, term6, MakeBinary);

            expression = term7;

            return expression.End();
        }

        protected abstract T False { get; }
        protected abstract T True { get; }

        protected abstract T Parse(string input);
        protected abstract T Negate(T v);
        protected abstract T Add(T a, T b);
        protected abstract T Sub(T a, T b);
        protected abstract T Mul(T a, T b);
        protected abstract T Div(T a, T b);
        protected abstract T Mod(T a, T b);
        protected abstract T Pow(T a, T b);
        protected abstract T Equal(T a, T b);
        protected abstract T NotEqual(T a, T b);
        protected abstract T LessThan(T a, T b);
        protected abstract T LessThanOrEqual(T a, T b);
        protected abstract T GreaterThan(T a, T b);
        protected abstract T GreaterThanOrEqual(T a, T b);
        protected abstract bool IsTrue(T v);

        protected abstract T Round(T v);
        protected abstract T Floor(T v);
        protected abstract T Ceiling(T v);
        protected abstract T Log10(T v);
        protected abstract T Sin(T v);
        protected abstract T Cos(T v);
        protected abstract T Tan(T v);
        protected abstract T Atan(T v);
        protected abstract T Atan2(T x, T y);
        /// <summary>
        /// Converts value in radians to value in degrees
        /// </summary>
        protected abstract T Rad2Deg(T v);
        /// <summary>
        /// Converts value in degrees to value in radians
        /// </summary>
        protected abstract T Deg2Rad(T v);
        /// <summary>
        /// Calculates the absolute value
        /// </summary>
        protected abstract T Abs(T v);
        /// <summary>
        /// Calculates the sign of the value
        /// </summary>
        /// <returns>-1 for negative values, 0 for zero, and 1 for positive values</returns>
        protected abstract T Sign(T v);

        protected abstract T Random(T minInclusive, T maxExclusive);
        protected abstract T RandomInt(T minInclusive, T maxExclusive);

        protected virtual T Log(T v, T newBase) => Div(Log10(v), Log10(newBase));

        private T Not(T v) => IsTrue(v) ? False : True;
        private T And(T a, T b) => IsTrue(a) ? b : a;
        private T Or(T a, T b) => IsTrue(a) ? a : b;
        private T Min(T a, T b) => IsTrue(GreaterThan(a, b)) ? b : a;
        private T Max(T a, T b) => IsTrue(GreaterThan(b, a)) ? b : a;

        private delegate Expression<T> ExprBuilder(ExpressionContext<T> context);

        private delegate T UnaryFunc(T a);

        private delegate T BinaryFunc(T a, T b);

        private ExprBuilder MakeFunction(string name, List<ExprBuilder> parameterBuilders)
        {
            switch (name)
            {
                case "NOT":
                    return MakeFunction1(Not);

                case "ROUND":
                    return MakeFunction1(Round);

                case "CEILING":
                    return MakeFunction1(Ceiling);

                case "FLOOR":
                    return MakeFunction1(Floor);

                case "LOG":
                    return parameterBuilders.Count == 2
                        ? MakeBinary(Log, parameterBuilders[0], parameterBuilders[1])
                        : MakeFunction1(Log10);
                
                case "SIN":
                    return MakeFunction1(Sin);
                
                case "COS":
                    return MakeFunction1(Cos);
                
                case "TAN":
                    return MakeFunction1(Tan);
                
                case "ATAN":
                    return MakeFunction1(Atan);
                
                case "ATAN2":
                    return MakeFunction2(Atan2);
                
                case "RAD2DEG":
                    return MakeFunction1(Rad2Deg);
                
                case "DEG2RAD":
                    return MakeFunction1(Deg2Rad);
                
                case "ABS":
                    return MakeFunction1(Abs);
                
                case "SIGN":
                    return MakeFunction1(Sign);
                
                case "RANDOM":
                    return MakeFunction2(Random);
                
                case "RANDOM_INT":
                    return MakeFunction2(RandomInt);

                case "MIN":
                    return MakeFunctionFold(Min);

                case "MAX":
                    return MakeFunctionFold(Max);

                case "IF":
                    if (parameterBuilders.Count < 3 ||
                        parameterBuilders.Count % 2 != 1)
                    {
                        throw new FunctionNotDefinedException(name, "Wrong parameters count");
                    }

                    return context =>
                    {
                        var conditions = new List<Expression<T>>();
                        var results = new List<Expression<T>>();
                        var defaultResult = parameterBuilders[parameterBuilders.Count - 1].Invoke(context);

                        for (var i = 0; i < parameterBuilders.Count - 1; i += 2)
                        {
                            conditions.Add(parameterBuilders[i].Invoke(context));
                            results.Add(parameterBuilders[i + 1].Invoke(context));
                        }

                        return () =>
                        {
                            for (var i = 0; i < conditions.Count; i++)
                            {
                                if (IsTrue(conditions[i].Invoke()))
                                {
                                    return results[i].Invoke();
                                }
                            }

                            return defaultResult.Invoke();
                        };
                    };

                default:
                    return MakeExternalFunction(name, parameterBuilders);
            }

            ExprBuilder MakeExternalFunction(string name, List<ExprBuilder> parameterBuilders)
            {
                return context =>
                {
                    var func = context.GetFunction(name);
                    if (func.ArgumentCount != parameterBuilders.Count)
                        throw new FunctionNotDefinedException(name, "Wrong parameters count");

                    var inner = new List<Expression<T>>();
                    for (var i = 0; i < parameterBuilders.Count; i++)
                        inner.Add(parameterBuilders[i].Invoke(context));

                    return func.Invoke(inner);
                };
}

            ExprBuilder MakeFunction1(Func<T, T> func)
            {
                if (parameterBuilders.Count != 1)
                {
                    throw new FunctionNotDefinedException(name, "Wrong parameters count");
                }

                return context =>
                {
                    var inner = parameterBuilders[0].Invoke(context);
                    return () => func(inner.Invoke());
                };
            }
            
            ExprBuilder MakeFunction2(BinaryFunc func)
            {
                if (parameterBuilders.Count != 2)
                {
                    throw new FunctionNotDefinedException(name, "Wrong parameters count");
                }

                return MakeBinary(func, parameterBuilders[0], parameterBuilders[1]);
            }

            ExprBuilder MakeFunctionFold(Func<T, T, T> func)
            {
                if (parameterBuilders.Count < 1)
                {
                    throw new FunctionNotDefinedException(name, "Wrong parameters count");
                }

                return context =>
                {
                    var inner = new List<Expression<T>>();

                    for (var i = 0; i < parameterBuilders.Count; i++)
                    {
                        inner.Add(parameterBuilders[i].Invoke(context));
                    }

                    return () =>
                    {
                        var result = inner[0].Invoke();
                        for (var i = 1; i < inner.Count; i++)
                        {
                            result = func(result, inner[i].Invoke());
                        }

                        return result;
                    };
                };
            }
        }

        private static ExprBuilder MakeUnary(UnaryFunc func, ExprBuilder innerBuilder)
        {
            return context =>
            {
                var inner = innerBuilder.Invoke(context);
                return () => func(inner.Invoke());
            };
        }

        private static ExprBuilder MakeBinary(BinaryFunc func, ExprBuilder l, ExprBuilder r)
        {
            return context =>
            {
                var left = l.Invoke(context);
                var right = r.Invoke(context);
                return () => func(left.Invoke(), right.Invoke());
            };
        }

        private ExprBuilder MakeVariable(string name)
        {
            if (name.Equals("TRUE", StringComparison.Ordinal))
            {
                return context => () => True;
            }

            if (name.Equals("FALSE", StringComparison.Ordinal))
            {
                return context => () => False;
            }

            return context =>
            {
                var variable = context.GetVariable(name);
                return variable;
            };
        }

        private static ExprBuilder MakeConstant(string valueString, Func<string, T> parser)
        {
            var value = parser(valueString);
            return context => () => value;
        }
    }

    public class ExpressionContext<T>
    {
        private readonly ExpressionContext<T> _parent;
        private readonly Func<string, Expression<T>> _unregisteredVariableResolver;
        private readonly Func<string, IFunction<T>> _functionResolver;

        private readonly Dictionary<string, Expression<T>> _variables = new Dictionary<string, Expression<T>>();

        public ExpressionContext(ExpressionContext<T> parent = null,
            Func<string, Expression<T>> unregisteredVariableResolver = null,
            Func<string, IFunction<T>> functionResolver = null)
        {
            _parent = parent;
            _unregisteredVariableResolver = unregisteredVariableResolver;
            _functionResolver = functionResolver;
        }

        public void RegisterVariable(string name, Expression<T> value)
        {
            if (_variables.ContainsKey(name))
            {
                throw new InvalidOperationException($"Variable {name} already registered");
            }

            _variables.Add(name, value);
        }

        public IFunction<T> GetFunction(string name)
        {
            var function = _functionResolver?.Invoke(name) ?? _parent?.GetFunction(name);
            if (function == null)
                throw new FunctionNotDefinedException(name, "Unknown name");

            return function;
        }

        public Expression<T> GetVariable(string name, bool nullIsOk = false)
        {
            if (_variables.TryGetValue(name, out var variable))
            {
                return variable;
            }

            if (_unregisteredVariableResolver != null)
            {
                variable = _unregisteredVariableResolver.Invoke(name);
                if (variable != null)
                {
                    return variable;
                }
            }

            var parentVariable = _parent?.GetVariable(name, nullIsOk: true);
            if (parentVariable != null)
            {
                return parentVariable;
            }

            if (nullIsOk)
            {
                return null;
            }

            throw new VariableNotDefinedException(name);
        }
    }

    [Obsolete("ExpresionContext contains a typo. Use ExpressionContext instead", true)]
    public class ExpresionContext<T> : ExpressionContext<T>
    {
    }

    public class VariableNotDefinedException : Exception
    {
        public VariableNotDefinedException(string name)
            : base($"Variable '{name}' not defined")
        {
        }
    }

    public class FunctionNotDefinedException : Exception
    {
        public FunctionNotDefinedException(string name, string reason) : base(
            $"Function '{name}' not defined: {reason}")
        {
        }
    }

    public class ExpressionParseException : Exception
    {
        public ExpressionParseException(string expression, ParseException parseException)
            : base($"Failed to parse expression '{expression}'{Environment.NewLine}{parseException.Message}")
        {
        }
    }
}