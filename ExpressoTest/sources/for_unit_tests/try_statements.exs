/**
 * Test program for try, catch statements
 */
module main;



class ExsException
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
        throwException();
    }
    catch ExsException{Message} {
        println(Message);
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
    catch ExsException{Message} {
        tmp2 = 2;
        println(Message);
    }
    finally{
        tmp2 = 3;
    }
    printFormat("tmp2 is {0} at last\n", tmp2);
}