# SecureRandom
Secure random number generation based on entropy conversion.

This produces a perfect unbiased random number sequence based on a cryptographically
secure entropy source such as RNGCryptoServiceProvider.

## Usage

```cs
using Entropy;

void Main()
{
    var random = new SecureRandom();
    
    // Roll a die 100 times
    for (int i=0; i<100; ++i)
        Console.WriteLine($"You rolled a {random.Next(1,7)}");
    
    // Random shuffle
    var deck = new int[52];
    random.Shuffle(deck);
}
```

## Theoretical background

This algorithm solves the fundamental problem "how do you shuffle a deck of cards using a string of binary entropy."

The fundamental problem is that the entropy from sources such as `RNGCryptoServiceProvider` only produce arrays of bytes,
and need to be converted into other bases before they can be used. This is extremely inconvenient, error-prone, and potentially inefficient.

It is important that the resulting numbers are unbiased, and when shuffling an array (such as a deck of cards), that the resulting array is "perfectly shuffled" meaning that all permutations are equally likely.

It is desirable to minimise the number of bytes read from the input stream, because
hardware entropy can be slow to generate, and because it is unsatisfactory from a theoretical perspective to read too much 
data from the input source.

In order to work with entropy, we store randomness in an "entropy buffer" consisting of a uniform random number "value" in the range [0, size).

To be continued...

