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

        [Test]
        public void PcgRandomStateTest()
        {
            var state1 = RandomState.FromSeed(1);
            var state2 = RandomState.FromSeed(2);

            Assert.AreNotEqual(state1.NextUint(), state2.NextUint());
            Assert.AreNotEqual(state1.NextFloat(), state2.NextFloat());
            Assert.AreNotEqual(state1.NextDouble(), state2.NextDouble());

            state1 = state2;

            Assert.AreEqual(state1.NextUint(), state2.NextUint());
            Assert.AreEqual(state1.NextFloat(), state2.NextFloat());
            Assert.AreEqual(state1.NextDouble(), state2.NextDouble());

            state1.NextUint();

            Assert.AreNotEqual(state1.NextUint(), state2.NextUint());
            Assert.AreNotEqual(state1.NextFloat(), state2.NextFloat());
            Assert.AreNotEqual(state1.NextDouble(), state2.NextDouble());
        }

        [Test]
        public void PcgSeedDistributionTest()
        {
            const int count = 1000;
            const int iterations = 100000;
            const int average = iterations / count;
            const int tolerance = average/2;

            var values = new int[count];

            for (int i = 0; i < iterations; ++i)
            {
                ulong state = PcgRandomAlgorithm.InitializeState((ulong)i);
                var value = PcgRandomAlgorithm.NextUint(ref state) % count;
                values[value]++;
            }

            var maxError = 0;
            for (int i = 0; i < count; ++i)
            {
                var diff = System.Math.Abs(values[i] - average);
                if (diff > maxError) 
                    maxError = diff;
            }

            UnityEngine.Debug.Log($"maxError = {maxError}/{tolerance}");
            Assert.LessOrEqual(maxError, tolerance);
        }

        [Test]
        public void PcgRandomTest()
        {
            var random1 = new PcgRandom(1, 11111111111);
            Assert.AreEqual(random1.Sequence, 11111111111);
            var random2 = new PcgRandom(1, 22222222222);
            Assert.AreEqual(random2.Sequence, 22222222222);
            var random3 = new PcgRandom(1, random1.Sequence);

            Assert.AreNotEqual(random1.NextUint(), random2.NextUint());
            Assert.AreNotEqual(random1.NextFloat(), random2.NextFloat());
            Assert.AreNotEqual(random1.NextDouble(), random2.NextDouble());

            Assert.AreNotEqual(random1.NextUint(), random3.NextUint());
            Assert.AreNotEqual(random1.NextFloat(), random3.NextFloat());
            Assert.AreNotEqual(random1.NextDouble(), random3.NextDouble());

            random3.State = random1.State;

            Assert.AreEqual(random1.NextUint(), random3.NextUint());
            Assert.AreEqual(random1.NextFloat(), random3.NextFloat());
            Assert.AreEqual(random1.NextDouble(), random3.NextDouble());

            random2.State = random3.State;
            random2.NextUint();
            random3.NextUint();

            Assert.AreNotEqual(random2.NextUint(), random3.NextUint());
            Assert.AreNotEqual(random2.NextFloat(), random3.NextFloat());
            Assert.AreNotEqual(random2.NextDouble(), random3.NextDouble());
        }
    }
}
