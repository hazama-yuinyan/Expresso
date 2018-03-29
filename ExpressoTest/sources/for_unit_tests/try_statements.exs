/**
 * Test program for try, catch statements
 */
module main;

import System.Exception as Exception;



def throwException()
{
    throw Exception{message: "An unknown error has occurred"};
}

def main()
{
    try{
        println("First try block");
        throwException();
    }
    catch e (- Exception {
        println("First catch block");
        println(e.Message);
    }

    /*var tmp = 1;
    try{
        println("tmp is ${tmp} at first");
        throwException();
    }
    finally{
        println("First finally block");
        tmp = 2;
    }
    println("tmp is ${tmp} at last");*/

    var tmp2 = 1;
    try{
        println("tmp2 is ${tmp2} at first");
        throwException();
    }
    catch e (- Exception {
        println("Second catch block");
        tmp2 = 2;
        println(e.Message);
    }
    finally{
        println("Second finally block");
        tmp2 = 3;
    }
    println("tmp2 is ${tmp2} at last");
}