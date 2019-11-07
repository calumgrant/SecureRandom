using System.Security.Cryptography;

namespace Tests
{
    class EntropyCounter : RandomNumberGenerator
    {
        public RandomNumberGenerator Source { get; }
        public int BytesRead { get; private set; }

        public int BitsRead => BytesRead * 8;

        public EntropyCounter(RandomNumberGenerator rng)
        {
            Source = rng;
        }
        public override void GetBytes(byte[] data)
        {
            Source.GetBytes(data);
            BytesRead += data.Length;
        }
    }
}
