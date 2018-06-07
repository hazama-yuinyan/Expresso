module main;


enum SomeEnum
{
    A = 1,
    B,
    C

    public def print()
    {
        match self {
            SomeEnum::A => println("A");,
            SomeEnum::B => println("B");,
            SomeEnum::C => println("C");
        }
    }

    public def printUsingIf()
    {
        if self == SomeEnum::A {
            println("A in if");
        }
    }
}

/*class SomeClass
{
    private let x (- SomeEnum;

    public def matchEnum()
    {
        match self.x {
            SomeEnum::A => println("A in matchEnum");,
            SomeEnum::B => println("B in matchEnum");,
            SomeEnum::C => println("C in matchEnum");
        }
    }

    public def ifEnum()
    {
        if self.x == SomeEnum::A {
            println("A in ifEnum");
        }
    }
}*/

def main()
{
    let enum_a = SomeEnum::A;

    match enum_a {
        SomeEnum::A => println("A");,
        SomeEnum::B => println("B");,
        SomeEnum::C => println("C");
    }

    enum_a.print();

    if enum_a == SomeEnum::A {
        println("A in if");
    }

    enum_a.printUsingIf();

    //let some_class = SomeClass{x: enum_a};
    //some_class.matchEnum();
    //some_class.ifEnum();
}