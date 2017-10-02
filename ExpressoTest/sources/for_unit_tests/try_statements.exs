/**
 * Test program for try, catch statements
 */
module main;

import "System.Exception" as Exception;



class ExsException : Exception
{
    public let ExsMessage (- string;
}

def throwException()
{
    throw ExsException{ExsMessage: "An unknown error has occurred"};
}

def main()
{
    try{
        println("First try block");
        throwException();
    }
    catch e (- ExsException {
        println("First catch block");
        println(e.ExsMessage);
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
    catch e (- ExsException {
        println("Second catch block");
        tmp2 = 2;
        println(e.ExsMessage);
    }
    finally{
        println("Second finally block");
        tmp2 = 3;
    }
    printFormat("tmp2 is {0} at last\n", tmp2);
}