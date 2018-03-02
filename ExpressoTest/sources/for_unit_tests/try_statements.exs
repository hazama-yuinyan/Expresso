/**
 * Test program for try, catch statements
 */
module main;

import "System.Exception" as Exception;



def throwException()
{
    throw Exception{Message: "An unknown error has occurred"};
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
        printFormat("tmp is {0} at first\n", tmp);
        throwException();
    }
    finally{
        println("First finally block");
        tmp = 2;
    }
    printFormat("tmp is {0} at last\n", tmp);*/

    var tmp2 = 1;
    try{
        printFormat("tmp2 is {0} at first\n", tmp2);
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
    printFormat("tmp2 is {0} at last\n", tmp2);
}