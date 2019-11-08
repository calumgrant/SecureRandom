using Xunit;
using Entropy;
using System.Security.Cryptography;

namespace Tests
{
    public class RandomTests
    {
        EntropyCounter Counter;
        SecureRandom RNG;

        public RandomTests()
        {
            Counter = new EntropyCounter(RandomNumberGenerator.Create());
            RNG = new SecureRandom(Counter);
        }

        [Fact]
        public void TestBits()
        {
            int sum = 0;
            for (int i = 0; i < 100; ++i)
                sum += RNG.Next(2);
            Assert.Equal(160, Counter.BitsRead);
        }

        [Fact]
        public void TestBytes()
        {
            int sum = 0;
            for (int i = 0; i < 100; ++i)
                sum += RNG.Next(256*256*256); // 3 bytes
            Assert.Equal(2432, Counter.BitsRead);
        }

        [Fact]
        public void TestDoubles()
        {
            double sum = 0.0;
            for (int i = 0; i < 100; ++i)
                sum += RNG.NextDouble();
            Assert.Equal(5208, Counter.BitsRead);
            Assert.True(sum < 60.0);
            Assert.True(sum > 40.0);
        }

        [Fact]
        public void TestShuffle()
        {
            int[] values = new int[52];

            for (int i = 0; i < 52; ++i) values[i] = i;
            RNG.Shuffle(values);
            int sum = 0;
            for (int i = 0; i < 52; ++i) sum += values[i];

            Assert.Equal(288, Counter.BitsRead);  // Has a low probability of failure
            Assert.Equal((51 * 52) / 2, sum);  // Basic test to see if it's shuffled.
        }
    }
}
