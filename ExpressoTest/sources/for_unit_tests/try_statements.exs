/**
 * Test program for try, catch statements
 */
module main;



class ExsException
{
    public Message (- string;
}

def throwException()
{
    throw ExsException{Message: "An unknown error has occurred"};
}

def main()
{
    try{
        throwException()
    }
    catch ExsException{Message} {
        println(Message);
    }
}