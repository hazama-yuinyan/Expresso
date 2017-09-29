/**
 * Test program for try, catch statements
 */
module main;



class ExsException : Exception
{
    public let Message (- string;
}

def throwException()
{
    throw ExsException{Message: "An unknown error has occurred"};
}

def main()
{
    try{
        println("First try block");
        throwException();
    }
    catch e (- ExsException {
        println(e.Message);
    }

    var tmp = 1;
    try{
        printFormat("tmp is {0} at first\n", tmp);
        throwException();
    }
    finally{
        tmp = 2;
    }
    printFormat("tmp is {0} at last\n", tmp);

    var tmp2 = 1;
    try{
        printFormat("tmp2 is {0} at first\n", tmp2);
        throwException();
    }
    catch e (- ExsException {
        tmp2 = 2;
        println(e.Message);
    }
    finally{
        tmp2 = 3;
    }
    printFormat("tmp2 is {0} at last\n", tmp2);
}