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
    catch(ex (- ExsException){
        println(ex.Message);
    }
}