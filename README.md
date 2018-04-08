# Expresso
-----------------

Expresso primarily aims to be an educational programming language. As such, it has similar syntaxes to PHP, and Java.
The language name __Expresso__ is coined by combining __Espresso__ with __expressive__, which means that it's easy for programmers to write codes in it as you can say "I can write a code while I'm having a cup of Espresso" and that it's highly expressive. And as such, Expresso is designed to provide high readability and perfomance at once.

## Characteristics

* Object-oriented(not mandatory, but highly recommended)
* Module based
* Builtin support for many useful types such as bigint, intseq, function and slice
* No traditional for statements
* Type-strict 
* Having collections in the built-in types
* Type inference
* Complete interoperability with other .NET languages at IL code level

## How to compile

First, `git clone` the repository and run `git submodule update --init`. Next download and move Coco(a Parser Generator for C#). If you are on Mac, then you can skip this step since `git clone` fetches a repository including the Coco. In order to download Coco, go to [here](http://www.ssw.uni-linz.ac.at/Coco/) and download `Coco.exe` in "Coco/R for C#" section. Unzip the downloaded file and move Coco.exe file to the root directory of Expresso *project*(that is the subdirectory of the root directory). Note that we're generating the parser using a shell script. So if you are on Windows, you will also have to write a batch file or something for generating the parser. And make a directory named "test_executables" in Expresso/ExpressoTest. In addition to those, you now have to get some NuGet pacakages. If you are using an IDE, it will be automatically downloaded if configured so. Finally, build the InteroperabilityTest project and move the resulting DLL to /ExpressoTest/sources/for_unit_tests/. This DLL is needed to run some tests. Then you should be ready to compile the projects.   
I've checked that it runs on Windows. However, most of the tests happily run on Windows but some don't, umm...
Just open up the solution on VS or whatever IDE you're using and build the 'ExpressoConsole' project. Since this project contains the front end executable, you can compile Expresso's source codes using the `exsc` command after that. Then all you have to do is to execute the binary produced by the `exsc` command with `mono`.
