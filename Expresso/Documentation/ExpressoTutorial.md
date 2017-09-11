* The Expresso tutorial
** Preface

Expresso primarily aims to be a programming language for education, or in other words another Pascal. And therefore Expresso
has two concepts. Expresso aims to be an "easy-to-write" and "expressive" programming language.
Even though we heavily put focus on syntax and the grammar, its policy reads "Easy for beginners, elegant for enthusisasts"
meaning that it's easy for beginners to read and write, and still it has lots of rich features that advanced programmers can take advantage of.
Even though it's a type-strict language, it looks rather in type-free way thanks to the very powerful type inference system.

** Hello, Expresso world!

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

This reads: Let's write an Expresso program where we're defining a module named "main" and a function also called "main".
The program will start executing from the "main" function, and inside which we're outputing...
Every Expresso program consists of at least one module and every module must be explicitly named.
Here, we name the module `main` because we usually call the top-level module that is the entry point for a program `main`. As
you progress reading the tutorial, you will see different names used for the module names.
In `main` function, which is the entry point for the program, we `println'ed` a string. `println` is a function that outputs
some string to the console. There are also the single line version, `print`, that doesn't output a line break at the end and
the format string version, `printFormat`, that formats the string according to the first argument given to that function.

The main policy for Expresso is that "Programming languages must allow programmers to write what it does, not how it does something".
In traditional C, we often end up writing something like the following:
```c
// construct some array of ints
// and name it array
for(int i = 0; i < sizeof(array) / sizeof(int); ++i){
    // do something on each element
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

** Main philosophies 
Expresso has two philosophies. One is "What the compiler doesn't allow you to do is just what you can't", and the other is
"Just do as the compiler says" or more hilariously, "Just lean and bend on the compiler until the perpendicular becomes the parallel".
The first one is somewhat obvious because otherwise, the compiler has some bugs.
The latter tells you that if you just follow what the compiler says, then you can do right things naturally.

** Let bindings and variable declarations
In Expresso, there are 2 forms of variable binding. 
Variables are useful considering the ability to keep track of values they hold. But sometimes we just want to give values descriptive names
because we need the same values several times or because it is tedious to change all the literal values over and over when you try
to guess the proper values for some programs. That's where constants come into play.
As a general term, a constant is a constant value meaning that values that are bound to variables will never be changed during program execution.
Let bindings introduce tags, and tags are names which you use in order to refer to the values later on.
By contrast, variable declarations introduce boxes that have certain shapes, and those boxes can be
filled with anything at any time as long as the shapes match.

** Statements
Like most curly-brace-and-semicolon-delimited languages, Expresso employs statements for its first language constructs.

** Expressions

** Tuples, vectors and dictionaries
In Expresso, it supports three basic data structures as builtin types. The biggest advantage of supporting vectors or dictionaries as builtin types
is the ability to support literal forms that help programmers make those objects easily and thoroughly.
And also, it is worth noting that the compiler can tell more specific and understandable error messages if vectors and dictionaries are builtin types.
Of course, it can even suggest the propper usages when it encounters some errors.
In contrast, it can be considered as a disadvantage that supporting those types as builtin makes it hard to maintain the source code and
it can lead to compiler size inflation.
