using NUnit.Framework;

namespace CodeWriter.ExpressionParser.Tests
{
    public class VariantTests
    {
        [Test]
        [TestCase("1", ExpectedResult = "1")]
        [TestCase("005", ExpectedResult = "5")]
        [TestCase("1.1", ExpectedResult = "1.1")]
        [TestCase("1.100", ExpectedResult = "1.1")]
        [TestCase("TRUE", ExpectedResult = "True")]
        [TestCase("FALSE", ExpectedResult = "False")]
        [TestCase("1 + 2.5 + 3 + 4.4 - 1", ExpectedResult = "9.9")]
        public string Parse(string input) => Execute(input, null).ToString();

        private static Variant Execute(string input, ExpressionContext<Variant> context)
        {
            return Compile(input, context).Invoke();
        }

        private static Expression<Variant> Compile(string input, ExpressionContext<Variant> context)
        {
            return VariantExpressionParser.Instance.Compile(input, context, false);
        }
    }
}