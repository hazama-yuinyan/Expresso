Expresso Language Specification 


Preface
The language name “Expresso” is coined as a mix of “Espresso” and “expressive”, meaning that it's easy for programmers to write a code in it like you're saying “I can write a code while I'm having a cup of Espresso” and that it's highly expressive.






In the context of the programming language ”Expresso”, equality and equivalence refer to different concepts. Here are the definitions of the terms.
Equality:
Assume there are objects a and b,  and if both are primitive types such as int, long etc. then a and b are considered to be equal when and only when both have the same value. If both a and b are class instances or struct instances then a and b are considered to be equal when and only when both have exactly the same structure and values in them. If a and b are 

Equivalence:
If objects a and b are primitive types, then they are considered equivalent when and only when both have the same value. 


When you define a type, think twice before implementing equality test. That is, whether instances of that class are supposed to be equal based on equality or equivalence.


In Expresso, all subroutines reside in a module or a class. We call the former a function and the latter a method. And as such all subroutines in Expresso takes a so-called “this” argument as the first argument. (Note that the “this” argument is hidden to the programmers as in C, C++, C# etc.)

The functions and methods in Expresso are first-class objects, which means any functions and methods can be bound to a variable and called in other contexts than the ones in which they are defined(but not every context can succeed the call where the call causes a runtime exception if the context is not suitable for the subroutine).

Since Expresso is a strongly-typed programming language, variables bound to function objects, too, have types and they are defined like the function signature. 