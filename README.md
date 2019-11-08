# SecureRandom
Secure random number generation based on entropy conversion.

This produces a perfect unbiased random number sequence based on a cryptographically
secure entropy source such as `RNGCryptoServiceProvider`.

## Usage

The class `Entropy.SecureRandom` is a drop-in replacement for `System.Random`. For example,

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

## Installation
`SecureRandom` is available as a [nuget package](https://www.nuget.org/packages/SecureRandom), and can be found using the package `SecureRandom`. For example,
```
dotnet add package SecureRandom --version 1.0.0
```

## Building

Checkout the code into a directory using
```
git clone https://github.com/calumgrant/SecureRandom
```
then build the code using
```
dotnet build
```
Tests can be run using
```
dotnet test
```

## Theoretical background

### Motivation

This algorithm solves the fundamental problem "how do you shuffle a deck of cards using a string of binary entropy."

The fundamental problem is that the entropy from sources such as `RNGCryptoServiceProvider` only produce arrays of bytes,
and need to be converted into other forms before they can be used. This is extremely inconvenient, error-prone, and potentially inefficient.

It is important that the resulting numbers are unbiased, and when shuffling an array (such as a deck of cards), that the resulting array is "perfectly shuffled" meaning that all permutations are equally likely.

**The theoretical guarantee offered by `SecureRandom` is that if the input entropy is unbiased, then the generated random numbers (and shuffles) are unbiased.**

It is desirable to minimise the number of bytes read from the input stream, because hardware entropy can be slow to generate,
and because it is unsatisfactory from a theoretical perspective to read too much data from the input source.

### Implementation
In order to work with entropy, we store randomness in an "entropy buffer" consisting of a uniform random number 
`value` in the range `[0, size)`. The number of bits of entropy stored in this buffer is `lg(size)`. If `size` is a 64-bit `ulong`, then the buffer can store up to 64 bits of entropy.

In order to extract some entropy from the buffer, we can perform the following operations on an entropy buffer:

- *Factor*: If `size = n*m`, then we can factor the entropy buffer into two smaller entropy buffers of size `n` and `m`.

```
    a = value/m;
    b = value%m;
```

- *Split*: If `size = a+b`, then we can spend up to 1 bit of entropy in order to produce an entropy buffer of size `a` or size `b`.

```
    if (value<a)
    {
        size = a;
    }
    else
    {
        value -= a;
        size = b;
    }
```

The algorithm used by `SecureRandom` is to 

1. Ensure a large entropy buffer by reading as much data as possible from the source into `value`.
2. Resize the buffer using *split* such that size = `n*m`.
3. *Factor* the buffer into the result (`value%n`) and the residue (`value/n`).

The efficency of this algorithm is extremely good, since the *split* operation loses very little entropy on average.
