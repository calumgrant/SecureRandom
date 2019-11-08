using System;
using System.Security.Cryptography;

namespace Entropy
{
    /// <summary>
    /// A perfect random number generator that efficiently converts random bits
    /// from an entropy source (such as RNGCryptoServiceProvider) into
    /// random numbers of an arbitrary range.
    /// 
    /// The benefit of this is that it
    /// - provides a more convenient interface for providing secure, unbiassed integers
    ///   of any range
    /// - very efficiently uses entropy from the entropy source.
    /// </summary>
    public class SecureRandom : Random, IDisposable
    {
        /// <summary>
        /// Creates a new SecureRandom using a 'RNGCryptoServiceProvider'
        /// </summary>
        public SecureRandom() : this(new RNGCryptoServiceProvider())
        {
        }

        /// <summary>
        /// Creates a new SecureRandom using a specified RandomNumberGenerator.
        /// </summary>
        /// <param name="rng">The entropy source.</param>
        public SecureRandom(RandomNumberGenerator rng)
        {
            source = rng;
            size = 1;
            value = 0;
        }

        public override int Next(int min, int max)
        {
            if (max <= min) throw new ArgumentException("max must be greater than min");

            return min + Next(max-min);
        }

        public override int Next() => (int)Next((ulong)int.MaxValue);

        public override int Next(int max) => (int)Next((ulong)max);

        private ulong Next(ulong v)
        {
                for (;;)
                {
                    // Step 1: Read more entropy into 'value'
                    // up to the capacity of a ulong.
                    int bytesToRead = 0;
                    while (size < (1L << 56))
                    {
                        bytesToRead++;
                        size <<= 8;
                    }

                    if (bytesToRead > 0)
                    {
                        byte[] buffer = new byte[8];
                        source.GetBytes(buffer, 0, bytesToRead);
                        var a = BitConverter.ToUInt64(buffer, 0);
                        value = value << (8 * bytesToRead) | a;
                    }

                    // Step 2: Extract the entropy from the 'value'
                    // We can do this perfectly when 'size % v = 0'
                    // in which case we can simply return 'value % v', divide
                    // 'size' by 'v' and we are finished.
                    // To handle the case where 'size %v != 0',
                    // we can resize 'size' by subtracting the remainder 'size % v'.
                    // This is a "split" operation where we either end up with
                    // a random number in the range [0, newSize) or
                    // a random number in the range [0, remainder).
                    // The "split" operation is lossy, but is generally extremely
                    // efficient because size >> v.
                    var remainder = size % v;
                    var newSize = size - remainder;

                    if (value < newSize)  // Entropy loss here
                    {
                        // Very high probability

                        ulong result = value % v;
                        value /= v;
                        size = newSize / v;
                        return result;
                    }
                    else
                    {
                        // Very low probability

                        value -= newSize;
                        size = remainder;
                    }
            }
        }

        public override double NextDouble()
        {
            // A "double" has 52 bits
            // We are bumping up to the limit of what the 'Next()' can
            // comfortably handle which is about 56 bits.
            const ulong N = 1UL << 52;
            var a = Next(N);
            return (double)a / (double)N;
        }

        protected override double Sample() => NextDouble();

        /// <summary>
        /// Securely shuffles the given array.
        /// </summary>
        /// <typeparam name="T">The element type of the array.</typeparam>
        /// <param name="array">The array to shuffle.</param>
        public void Shuffle<T>(T[] array)
        {
            // Fischer-Yates sorting algorithm
            for(int i=0; i<array.Length-1; ++i)
            {
                int j = Next(i, array.Length);
                var t = array[i];
                array[i] = array[j];
                array[j] = t;
            }
        }

        /// <summary>
        /// Fills an array with random numbers from the entropy source.
        /// </summary>
        /// <param name="values">The array to fill.</param>
        public override void NextBytes(byte[] values)
        {
            source.GetBytes(values);
        }

        public void Dispose()
        {
            source.Dispose();
        }

        private ulong value;  // A random number in the range [0,size)
        private ulong size;   // Range of 'value'

        private readonly RandomNumberGenerator source;
    }
}
