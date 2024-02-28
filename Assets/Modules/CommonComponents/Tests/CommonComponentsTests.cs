using NUnit.Framework;

namespace CommonComponents.Tests
{
	[TestFixture]
	public class CommonComponentsTests
	{
		[Test]
		[TestCase(100000000000000, 100000000000000, ExpectedResult = 200000000000000)]
		[TestCase(long.MaxValue, long.MaxValue, ExpectedResult = long.MaxValue)]
		public long MoneyAddTest(long first, long second)
		{
			var result = (Money)first + second;
			return (long)result;
		}

		[Test]
		[TestCase(1000, 100, ExpectedResult = 900)]
		[TestCase(100, 1000, ExpectedResult = 0)]
		public long MoneySubTest(long first, long second)
		{
			var result = (Money)first - second;
			return (long)result;
		}

		[Test]
		[TestCase(100000000, 100000000, ExpectedResult = 10000000000000000)]
		[TestCase(100000000000000, 100000000000000, ExpectedResult = long.MaxValue)]
		[TestCase(long.MaxValue, long.MaxValue, ExpectedResult = long.MaxValue)]
		public long MoneyMulTest(long first, long second)
		{
			var result = (Money)first * second;
			return (long)result;
		}
        }
}
