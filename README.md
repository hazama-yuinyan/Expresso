# Expresso
-----------------

Expresso primarily aims to be an educational programming language. As such, it has similar syntaxes to PHP, and Java.
The language name __Expresso__ is coined as a mixture of __Espresso__ and __expressive__, meaning that it's easy for programmers to write a code in it like you're saying "I can write a code while I'm having a cup of Espresso" and that it's highly expressive. And as such, Expresso is designed to provide high readability and perfomance at once.

## Characteristics

* Object-oriented(not mandatory, but highly recommended)
* Module based
* Builtin support for many useful types such as bigint, intseq, function and slice
* No traditional for statements
* Type-strict 

## How to compile

First, `git clone` the repository and run `git submodule update --init`. Next download and move Coco(a Parser Generator for C#). In order to download Coco, go to [here](http://www.ssw.uni-linz.ac.at/Coco/) and download `Coco.exe` in "Coco/R for C#" section. Unzip the downloaded file and move Coco.exe file to the root directory of Expresso. Note that we're generating the parser using a shell script. So if you are on Windows, you will have to write a batch file or something for parser generation. Then you should be ready to compile the projects.   
Just open up the solution on VS or whatever IDE you're using and build the 'ExpressoConsole' project. Since this project contains the front end executable, you can compile Expresso's source codes using the `exsc` command after that. Then all you have to do is to execute the binary produced by the `exsc` command.
