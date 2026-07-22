using NUnit.Framework;

namespace CodeWriter.ExpressionParser.Tests
{
    public class ParserTests
    {
        [Test]
        // Constants
        [TestCase("1", ExpectedResult = 1)]
        [TestCase("123", ExpectedResult = 123)]
        [TestCase("12.0", ExpectedResult = 12)]
        [TestCase("12.34", ExpectedResult = 12.34f)]
        [TestCase("TRUE", ExpectedResult = 1)]
        [TestCase("FALSE", ExpectedResult = 0)]
        // Expressions
        [TestCase("(1)", ExpectedResult = 1)]
        [TestCase("(-1)", ExpectedResult = -1)]
        [TestCase("-(-1)", ExpectedResult = 1)]
        [TestCase("1 + 2", ExpectedResult = 3)]
        [TestCase("1+2", ExpectedResult = 3)]
        [TestCase(" 1  +   2    ", ExpectedResult = 3)]
        [TestCase("6 + 2", ExpectedResult = 8)]
        [TestCase("6 - 2", ExpectedResult = 4)]
        [TestCase("6 * 2", ExpectedResult = 12)]
        [TestCase("6 / 2", ExpectedResult = 3)]
        [TestCase("6 % 2", ExpectedResult = 0)]
        [TestCase("6 ^ 2", ExpectedResult = 36)]
        [TestCase("1 + 2 + 3 + 4", ExpectedResult = 10)]
        [TestCase("1 + 2 * 3", ExpectedResult = 7)]
        [TestCase("1 + (2 * 3)", ExpectedResult = 7)]
        [TestCase("(1 + (2 * 3))", ExpectedResult = 7)]
        [TestCase("(1 + 2) * 3", ExpectedResult = 9)]
        [TestCase("5 + 4 * 3 ^ 2", ExpectedResult = 41)]
        [TestCase("2 ^ 3 * 4 + 5", ExpectedResult = 37)]
        [TestCase("5 * 4 + 3 ^ 2", ExpectedResult = 29)]
        [TestCase("-2^2", ExpectedResult = 4)]
        [TestCase("-2^-2", ExpectedResult = 0.25f)]
        // NOT
        [TestCase("NOT(0)", ExpectedResult = 1)]
        [TestCase("NOT(1)", ExpectedResult = 0)]
        [TestCase("NOT(-5)", ExpectedResult = 0)]
        [TestCase("NOT(5)", ExpectedResult = 0)]
        [TestCase("NOT(NOT(0))", ExpectedResult = 0)]
        //ROUND
        [TestCase("ROUND(0)", ExpectedResult = 0)]
        [TestCase("ROUND(0.1)", ExpectedResult = 0)]
        [TestCase("ROUND(0.9)", ExpectedResult = 1)]
        [TestCase("ROUND(-0.1)", ExpectedResult = 0)]
        [TestCase("ROUND(-0.9)", ExpectedResult = -1)]
        //CEILING
        [TestCase("CEILING(0)", ExpectedResult = 0)]
        [TestCase("CEILING(0.1)", ExpectedResult = 1)]
        [TestCase("CEILING(0.9)", ExpectedResult = 1)]
        [TestCase("CEILING(-0.1)", ExpectedResult = 0)]
        [TestCase("CEILING(-0.9)", ExpectedResult = 0)]
        // LOG
        [TestCase("LOG(10)", ExpectedResult = 1)]
        [TestCase("LOG(10, 10)", ExpectedResult = 1)]
        [TestCase("LOG(10000, 10)", ExpectedResult = 4)]
        [TestCase("LOG(8, 2)", ExpectedResult = 3)]
        [TestCase("ROUND(LOG(1728, 12))", ExpectedResult = 3)]
        [TestCase("ROUND(LOG(0.0001, 0.01))", ExpectedResult = 2)]
        //FLOOR
        [TestCase("FLOOR(0)", ExpectedResult = 0)]
        [TestCase("FLOOR(0.1)", ExpectedResult = 0)]
        [TestCase("FLOOR(0.9)", ExpectedResult = 0)]
        [TestCase("FLOOR(-0.1)", ExpectedResult = -1)]
        [TestCase("FLOOR(-0.9)", ExpectedResult = -1)]
        // MIN
        [TestCase("MIN(5)", ExpectedResult = 5)]
        [TestCase("MIN(0, 0)", ExpectedResult = 0)]
        [TestCase("MIN(0, 1)", ExpectedResult = 0)]
        [TestCase("MIN(-5, 5)", ExpectedResult = -5)]
        [TestCase("MIN(5, 4, 7, 3, 4)", ExpectedResult = 3)]
        // MAX
        [TestCase("MAX(5)", ExpectedResult = 5)]
        [TestCase("MAX(0, 0)", ExpectedResult = 0)]
        [TestCase("MAX(0, 1)", ExpectedResult = 1)]
        [TestCase("MAX(-5, 5)", ExpectedResult = 5)]
        [TestCase("MAX(5, 4, 7, 3, 4)", ExpectedResult = 7)]
        // AND
        [TestCase("0 AND 0", ExpectedResult = 0)]
        [TestCase("0 AND 1", ExpectedResult = 0)]
        [TestCase("1 AND 0", ExpectedResult = 0)]
        [TestCase("1 AND 1", ExpectedResult = 1)]
        // OR
        [TestCase("0 OR 0", ExpectedResult = 0)]
        [TestCase("0 OR 1", ExpectedResult = 1)]
        [TestCase("1 OR 0", ExpectedResult = 1)]
        [TestCase("1 OR 1", ExpectedResult = 1)]
        // Compare
        [TestCase("1 == 1", ExpectedResult = 1)]
        [TestCase("1 == 2", ExpectedResult = 0)]
        [TestCase("1 != 1", ExpectedResult = 0)]
        [TestCase("1 != 2", ExpectedResult = 1)]
        [TestCase("1 < 2", ExpectedResult = 1)]
        [TestCase("1 <= 2", ExpectedResult = 1)]
        [TestCase("1 > 2", ExpectedResult = 0)]
        [TestCase("1 >= 2", ExpectedResult = 0)]
        [TestCase("2 < 2", ExpectedResult = 0)]
        [TestCase("2 <= 2", ExpectedResult = 1)]
        [TestCase("2 > 2", ExpectedResult = 0)]
        [TestCase("2 >= 2", ExpectedResult = 1)]
        [TestCase("3 < 2", ExpectedResult = 0)]
        [TestCase("3 <= 2", ExpectedResult = 0)]
        [TestCase("3 > 2", ExpectedResult = 1)]
        [TestCase("3 >= 2", ExpectedResult = 1)]
        // Logical
        [TestCase("4 AND 5", ExpectedResult = 5)]
        [TestCase("0 AND 5", ExpectedResult = 0)]
        [TestCase("4 OR 5", ExpectedResult = 4)]
        [TestCase("0 OR 5", ExpectedResult = 5)]
        // Logical
        [TestCase("1 >= 0 AND 2 < 3", ExpectedResult = 1)]
        [TestCase("0 >= 2 OR -4 < -5", ExpectedResult = 0)]
        [TestCase("1 > 0 AND 5", ExpectedResult = 5)]
        [TestCase("1 > 0 OR 5", ExpectedResult = 1)]
        [TestCase("1 > 2 AND 5", ExpectedResult = 0)]
        [TestCase("1 > 2 OR 5", ExpectedResult = 5)]
        [TestCase("1 AND 0 OR 1", ExpectedResult = 1)]
        [TestCase("1 AND 0 OR 0", ExpectedResult = 0)]
        [TestCase("1 AND (0 OR 1)", ExpectedResult = 1)]
        public float Parse(string input) => Execute(input, null);

        [Test]
        [TestCase("a", 1, 2, ExpectedResult = 1)]
        [TestCase("b", 1, 2, ExpectedResult = 2)]
        [TestCase("a + b", 3, 4, ExpectedResult = 7)]
        [TestCase("b ^ a + 1", 3, 4, ExpectedResult = 65)]
        [TestCase("a AND NOT(b)", 1, 0, ExpectedResult = 1)]
        [TestCase("NOT(a) AND NOT(b)", 1, 0, ExpectedResult = 0)]
        public float Parse_Variable(string input, float a, float b)
        {
            var context = new ExpressionContext<float>();
            context.RegisterVariable("a", () => a);
            context.RegisterVariable("b", () => b);
            return Execute(input, context);
        }

        [Test]
        public void ComplexVariableName()
        {
            var context = new ExpressionContext<float>();
            context.RegisterVariable("Some_Variable", () => 1);
            Assert.AreEqual(1, Execute("Some_Variable", context));
        }

        [Test]
        public void ReadmeSample()
        {
            var context = new ExpressionContext<float>();

            context.RegisterVariable("a", () => 1);
            context.RegisterVariable("b", () => 2);
            context.RegisterVariable("c", () => 3);

            var input = "a >= b AND NOT(b) OR (a + b) >= c";
            var compiledExpr = FloatExpressionParser.Instance.Compile(input, context, true);
            var result = compiledExpr.Invoke();

            Assert.AreEqual(1, result);
        }

        [Test]
        public void Parse_Variable_Names()
        {
            var context = new ExpressionContext<float>();
            context.RegisterVariable("o", () => 1);
            context.RegisterVariable("one", () => 1);
            context.RegisterVariable("one123", () => 1);
            context.RegisterVariable("_one123", () => 1);
            context.RegisterVariable("one_123", () => 1);
            context.RegisterVariable("one123_", () => 1);
            Assert.AreEqual(1, Execute("o", context));
            Assert.AreEqual(1, Execute("one", context));
            Assert.AreEqual(1, Execute("one123", context));
            Assert.AreEqual(1, Execute("_one123", context));
            Assert.AreEqual(1, Execute("one_123", context));
            Assert.AreEqual(1, Execute("one123_", context));
            Assert.Throws<ExpressionParseException>(() => Compile("123one", context));
            Assert.Throws<VariableNotDefinedException>(() => Compile("b", context));
        }

        [Test]
        public void Parse_Compare_Invalid()
        {
            Assert.Throws<ExpressionParseException>(() => Compile("0 > 0 > 0", null));
            Assert.Throws<ExpressionParseException>(() => Compile("0 <= 0 > 0", null));
            Assert.Throws<ExpressionParseException>(() => Compile("0 = 0 != 0", null));
        }

        [Test]
        public void Parse_MinMax_Invalid()
        {
            Assert.Throws<ExpressionParseException>(() => Compile("MIN()", null));
            Assert.Throws<ExpressionParseException>(() => Compile("MAX()", null));
        }

        [Test]
        public void Parse_If()
        {
            var context = new ExpressionContext<float>();

            var n = 0;
            context.RegisterVariable("n", () => n);

            var expr = Compile("IF(n < 1, 1, n < 5, 5, n < 10, 10, 20)", context);

            Assert.AreEqual(1, expr.Invoke());

            n = 4;
            Assert.AreEqual(5, expr.Invoke());

            n = 9;
            Assert.AreEqual(10, expr.Invoke());

            n = 15;
            Assert.AreEqual(20, expr.Invoke());

            Assert.Throws<FunctionNotDefinedException>(() => Compile("IF(1)", context));
            Assert.Throws<FunctionNotDefinedException>(() => Compile("IF(1, 1)", context));
            Assert.Throws<FunctionNotDefinedException>(() => Compile("IF(1, 1, 1, 1)", context));
        }

        [Test]
        public void ContextTree()
        {
            var rootContext = new ExpressionContext<float>();
            var subContext = new ExpressionContext<float>(rootContext);

            var rootA = 1;
            var rootB = 2;
            var subB = 3;

            rootContext.RegisterVariable("a", () => rootA);
            rootContext.RegisterVariable("b", () => rootB);
            subContext.RegisterVariable("b", () => subB);

            var expr = Compile("a + b", subContext);

            Assert.AreEqual(4, expr.Invoke()); // 1 + 3

            rootA = 10;
            Assert.AreEqual(13, expr.Invoke()); // 10 + 3

            subB = 20;
            Assert.AreEqual(30, expr.Invoke()); // 10 + 20

            rootB = 100;
            Assert.AreEqual(30, expr.Invoke()); // 10 + 20
        }

        [Test]
        [TestCase("0", ExpectedResult = false)]
        [TestCase("1", ExpectedResult = true)]
        [TestCase("5", ExpectedResult = true)]
        public bool Parse_Predicate(string input)
        {
            return FloatExpressionParser.Instance.CompilePredicate(input, null, false).Invoke();
        }

        private static float Execute(string input, ExpressionContext<float> context)
        {
            return Compile(input, context).Invoke();
        }

        private static Expression<float> Compile(string input, ExpressionContext<float> context)
        {
            return FloatExpressionParser.Instance.Compile(input, context, false);
        }
    }
}