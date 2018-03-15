# The Expresso tutorial

## Preface

Expresso primarily aims to be a programming language for education, or in other words another Pascal. And therefore Expresso
has two concepts. Expresso aims to be an "easy-to-write" and "expressive" programming language.
Even though we heavily put focus on syntax and the grammar, its policy reads "Easy for beginners, elegant for enthusisasts",
meaning that it's easy for beginners to read and write, and still it has lots of rich features that advanced programmers can take advantage of.
Even though it's a type-strict language, it looks rather in type-free way thanks to the very powerful type inference system.

## Hello, Expresso world!

OK, enough with pre-words. By far, we are all armed enough with concepts, goals and something like that so let's get started.
First of all, we starts from the traditional "hello world" program.
You may expect it to be a one-liner program considering the fact that I've mentioned Expresso looks rather like a scripting language,
but unfortunately it's two liners like the following:

```expresso
module main;
def main(){
    println("Hello, world!");
}
```

<span class="caption">Listing 1: A traditional hello, world! program in Expresso</span>

This reads: Let's write an Expresso program where we're defining a module named "main" and a function also called "main".
The program will start executing from the "main" function, and inside which we're outputing...

Every Expresso program consists of at least one module and every module must be explicitly named.
Here, we name the module `main` because we usually call the top-level module that is the entry point for a program `main`. As
you progress reading the tutorial, you will see different names used for the module names.
In Expresso the naming convention for modules is the snake case. In other words, you name a module "test_module" rather than
"testModule" or "TestModule". This is not a strict rule that the compiler enforces but it's considered a good style to keep that rule.
In `main` function, which is the entry point for the program, we `println'ed` a string. `println` is a function that outputs
some string to the console. There are also the single line version, `print`, that doesn't output a line break at the end and
the format string version, `printFormat`, that formats the string according to the first argument given to that function.

## Getting into the Expresso world

Let's dive into the Expresso world. Because I mentioned at the beginning of this guide that Expresso looks a type-free language rather than a type-strict one,
we'll use Expresso as a simple calculator first. 
Note that from now on we'll not be including the `module main; def main(){` part because with it the examples will be too complex. Since it's boring to repeat
writing the same thing over and over and it's considered hard to see if we include the magic header every time, we won't do that.
So if you follow this tutorial don't forget to include the magic header at the beginning of source files.
As the saying goes "A journey of a thousand miles begins with a single step", we'll be starting with a simple calculation.

```expresso
println(1 + 1);
```

This will print `2` as you might be answering.
Next, a bit more complex.

```expresso
println(4 * 5);
```

This will print `20` as you may expect.
A piece of cake? Indeed. Then add some complexity.

```expresso
println(4 * 5 + 1);
```

<span class="caption">Listing 2: An expression with multiplication and addition</span>

This will print `21`.
It's still a piece of cake. As you might see, the evaluation will respect the mathematical operator precedences. That is, multiplication first and then addition.
Likewise, it will follow the same rules for other operators such as exponentiation.

> About exponentiation
> In Expresso, you can calculate the exponentiation with `**`. The result of executing `let a = 2 ** 10` will be `1024`.

OK, then we'll be doing the above calculation in a slightly different way. By using a variable.

```expresso
let a (- int = 4 * 5;
println(a + 1);
```

<span class="caption">Listing 3: Expressing Listing 2 in another way</span>

### Let bindings and variable declarations

In Expresso, there are 2 forms of variable binding. 
Variables are useful considering the ability to keep track of values they hold. But sometimes we just want to give values descriptive names
because we need the same values several times or because it is tedious to change all the literal values over and over when you try
to guess the proper values for some programs. That's where immutable variables come into play.
As a general term, an immutable variable is a constant value meaning that values that are bound to variables will never be changed during program execution.
Let bindings introduce tags, and tags are names which you use in order to refer to the values later on and immutable variables are considered to be the tags.
By contrast, variable declarations introduce boxes that have certain shapes, and those boxes can be
filled in with anything at any time as long as the shapes match.

In Listing 3, we introduced a new let binding stating `let n (- int = 4 * 5;`. This reads: We'll introduce a new let binding named 'n' whose type is `int` and
the value of it will be `4 * 5`. Note that we explicitly annotate the type here. Because Expresso has a strong type inference system, you woudln't usually need
to do so. But we'll generally do that here in the tutorial for clarity.

Note that we use `(-` for separating the variable name and the type. It represents `∈` in ASCII-compatible characters and means exactly the mathematical `∈`.
In other words, the right hand side includes the left hand side.

By introducing let bindings, we can keep the results of some operations aside. And then we can perform other operations based on those values. I would suggest 
you to prefer let bindings over variable declarations because let bindings tend to make the code clearer and more concise by making the code easier to read and
to follow the logic.

OK, enough with simple calculations. Next, let's look at the data types that variables are in.

Like other programming languages, Expresso has some types built into it. Namely the `int`, `uint` types for signed and unsigned integers, the `bool` type for
the booleans, the `float` and `double` types for single floating-point numbers and double floating-point numbers and the `char` for characters, etc.
Let's explore each type to see what it does.

## Data types

### The `int`, `uint` and `byte` types

The first types that we'll look at is the `int`, `uint` and `byte` types. Expresso provides just these three types for integers because it's considered to be used as
a scripting language rather than a type-strict language where would need more control over the size of integers.

As internal representations, `int`, `uint` and `byte` use `int`, `uint` and `byte` types on C#, respectively, so see API documentations for more informations on those
types.

### The `float` and `double` types

Next up is the `float` and `double` types. Like the above types, they are defined on C# as well.
That's all for floating-point numbers.

### The `bool` type

Next, let's look at a simple type: the `bool` type. The `bool` represents the boolean and has two possible values: true and false.
In Expresso, the `if` statement doesn't allow the conditional to have any types other than `bool`. If you are trying to do that,
you'll see the following error:

```
Error ES4000: The conditional expression has to be of type `bool`
```

The error message should tell the story.

### The `char` type

The `char` type represents a character. As usual, it is the same as the C#'s char type, so see the API documentation for more informations on the type.
But it is worth noting that the C#'s char type (and thus the `char` type) is encoded in UTF-16. To put it simply, a `char` can represent any character in UTF-16.
It's wonderful considering the fact that in the traditional C most of the characters aren't represented in one char.

### The `bigint` type

In the computer world, numbers are approximated using two's complement representation or are limited in size. For example, the `int` type can only represent integers
from -(2<sup>31</sup>) to 2<sup>31</sup> - 1 inclusive.
But in some fields, you'll need to represent numbers more properly, for example, when dealing with money. For those situations Expresso provides the `bigint` type.
A `bigint` can store any arbitrary integer. This makes the `bigint` type suitable for dealing with money.

### The `string` type

I realize that `char` and `string` types are the fundamental types because sometimes you would even need more string manipulations than you would integers.
As with the `char` type, the `string` type also is in UTF-16. See the C#'s documantation for other details on the `string` type.

### The `intseq` type

One unique characteristic for Expresso is the built-in `intseq` type. As the name suggests, it produces a series of integers.
The `intseq` type has 3 fields, `lower`, `upper` and `step`. `lower` represents the lower bound of the sequence,
`upper` the upper bound and `step` the step by which an iteration proceeds at a time.
The `intseq` type has the corresponding literal form and it is written as follows:
`lower(..|...)upper[:step]`

```expresso
    let seq = 1..10;    // `step` can be omitted and 1 is assumed if ommited and the double dots mean that the lower bound is
                        // inclusive but the upper bound is exclusive
    let series = seq.select(|x| => x);
    for let elem in seq {  // print "123456789"
        print(elem);
    }
    println(series); // print "[1,2,3,4,5,6,7,8,9...]"
```

<span class="caption">Listing4: An intseq turns into a vector</span>

An integer sequence expression does not create a vector of integers by itself. Instead, it creates a new object that is ready
to produce integers that are in the range specified in the expression.

```expresso
    let negative_seq = -10..0;
    let to_negative = 0..-10:-1;
    println("Legend: (f(x) = x - 10, g(x) = -x)");
    for let (x, to_n) in negative_seq.zip(to_negative) {
        print("(f(x), g(x)) = ({0}, {1}),", x, to_n);  // print "(f(x), g(x)) = (-10, 0),(f(x), g(x)) = (-9, -1)" and so on
        if x == to_n {      // and when it reaches (5, 5), it also prints "Crossed over!"
            println("Crossed over!");
        }
    }
```

<span class="caption">Listing5: Two graphs crossed over</span>

We call such objects iterators because they iterate through a sequence-like object and yields an element at a time.
It's very useful and it's one of the reasons that gives Expresso the power of expressive-ness.
An integer sequence expression can take negative values in any of its operands as long as they fit in the range of
the built-in `int` type(which corresponds to [-2<sup>31</sup>, 2<sup>31</sup> - 1]).
You may notice that the integer sequence expression looks like something, say, the conditional operator. And yes, that's right! 
In Expresso, we have 2 types of ternary operators. One is the conditional operator(often called "the ternary operator"
since most programming languages does have only one ternary operator) and the other is the integer sequence operator we have just introduced.

### The `slice` type

So far you may be sick of the tiring and boring explanations. But when combined with sequence types such as arrays or vectors,
the `intseq` type reveals its funny yet powerful potential out to the public.

```expresso
    let some_array = (0..10).toList();
    let first_half = some_array[0..5];
    let second_half = some_array[5..10];
    for let (a, b) in first_half.zip(second_half) {
        print("({0}, {1}),", a, b);   // print "(0, 5),(1, 6),(2, 7)" and so on
    }
```

<span class="caption">Listing6: Spliting into 2 slices</span>

In the above example, it seems that the latter 2 intseq objects extract elements from the same array object.
You may be wondering that it is inefficient because it seems that we have 3 arrays in the end. Having 3 arrays means that
Expresso first allocates 3 chunks of memory and fills the first chunk of memory with integers from 0 to 10, and then it copies
the first half of elements of the previous array to the second chunk of memory and the second half of elements to the last chunk of memory.
But Expresso is smart enough to solve this problem. Instead of returning a new array, it returns iterators that goes through
the first half of the elements and the second half of the elements respectively.
Note that the chunks of memory(`first_half` and `second_half` variables in this snippet) are called `slice` in Expresso and that `slice` is another iterator
(in .NET term it is also called an `enumerator`).

Sometimes, it's useful to view into some sequence. That's the time when the `slice` type comes into play. The `slice` type, as the name implies,
slices some sequence and allows you to view a portion of that sequence. Combining some sequence with an `intseq` using the indexer syntax
creates a new `slice` object. The slice object, then, can be used to iterate through some portion of the sequence.
Note that the `slice` is just an iterator of the sequence. Thus the `slice` object doesn't create a new copy of the original sequence.

### The `tuple` type

Sometimes, you may want to return more than 1 variable from functions. In other times, you may want to combine two values into some grouped construct.
For those situations, we have the `tuple` type as a builtin type. The `tuple` type conceptually groups more than 1 value into one construct and allows you to
move it around and pass it around.

### The `vector` type

In programs, you would often want to store multiple values of one type in one place. That's the time when the `vector` type comes into play. The `vector` type 
allows you to put multiple values of one type in an object like the following.

```expresso
var natural_numbers = [0, 1, 2, 3, 4, 5, ...];
```

<span class="caption">Listing 7: Initializing a vector of natural numbers</span>

Note the trailing periods. If you forget them, the object will be an array, which doesn't allow you to grow or shrink its size. As you can see,
Expresso allows you to construct an vector object in literal form, which most other type-strict programming languages don't.   
You can add or remove an item from the vector.

```expresso
natural_numbers.add(6);
println(natural_numbers);
natural_numbers.remove(6);
println(natural_numbers);
```

<span class="caption">Listing 8: Several uses of the vector's methods</span>

For other methods available, see the API documentation for the .NET's List<T> class. Note that we still can't call extension methods.

### The `array` type

The `array` type is another sequence type, which doesn't allow you to grow or shrink its size. Thus the compiler already knows the size of an array object
when compiling. However, it currently doesn't take advantage of it.

### The `dictionary` type



OK, next up is exponentiation. But we'll be doing it in a slightly different way. Even though Expresso has the operator for it, here we'll be doing it on our
own, using while loop.

The main policy for Expresso is that "Programming languages must allow programmers to write what it does, not how it does something".
In traditional C, we often end up writing something like the following:

```c
// construct some array of ints
// and name it array
for(int i = 0; i < sizeof(array) / sizeof(int); ++i){
    // do something on each element to transform them
}
```

Even though the for loop has long long history, I think that it doesn't express one's intension very clearly especially
when you want to process an array. Instead, I recommend you to use functional style. In functional programming languages,
we define the work flow as a chain of functions. So in Expresso, you can rewrite the above example like this:

```expresso
// construct some array
// and assume the array is named "a"
let mapped = a.map(|elem| => /* do something on each element */);
```

This reads: We're assigining the value that will be computed by iterating through the array and performing some calculations on each element of
the array to a variable. This sounds more straightforward and is easier to read, isn't it?

## Main philosophies

Expresso has two philosophies. One is "What the compiler doesn't allow you to do is just what you can't", and the other is
"Just do as the compiler says" or more hilariously, "Just lean and bend on the compiler until the perpendicular becomes the parallel".
The first one is somewhat obvious because otherwise, the compiler has some bugs.
The latter tells you that if you just follow what the compiler says, then you can do right things naturally.

## Statements

Like most curly-brace-and-semicolon-delimited languages, Expresso employs statements for its first language constructs.

## Expressions

## Tuples, vectors and dictionaries

Expresso supports three basic data structures as builtin types. The biggest advantage of supporting vectors or dictionaries as builtin types
is the ability to support literal forms that help programmers make those objects easily and thoroughly.
And also, it is worth noting that the compiler can tell more specific and understandable error messages if vectors and dictionaries are builtin types.
Of course, it can even suggest the propper usages when it encounters some errors.
In contrast, it can be considered as a disadvantage that supporting those types as builtin makes it hard to maintain the source code and
it can lead to compiler size inflation.
