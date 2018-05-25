module main;


enum SomeEnum
{
    A = 1,
    B,
    C
}

def main()
{
    let enum_a = SomeEnum::A;

    match enum_a {
        SomeEnum::A => println("A");,
        SomeEnum::B => println("B");,
        SomeEnum::C => println("C");
    }
}