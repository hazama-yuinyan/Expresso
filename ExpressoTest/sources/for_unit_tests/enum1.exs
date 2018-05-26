module main;


enum Union
{
    A(),
    B(int, uint),
    C(string, char),
    D(intseq)
}

def main()
{
    let some_enum = Union::A{};
    let some_enum2 = Union::B{0: 1, 1: 2u};

    println("${some_enum}");
    println("${some_enum2}");

    match some_enum {
        Union::A{} => println("A");,
        Union::B{some_int, some_uint} => println("${some_int}, ${some_uint}");,
        Union::C{some_str, some_char} => println("${some_str}, ${some_char}");,
        Union::D{some_intseq} => println("${some_intseq}");
    }

    match some_enum2 {
        Union::A{} => println("A");,
        Union::B{some_int, some_uint} => println("${some_int}, ${some_uint}");,
        Union::C{some_str, some_char} => println("${some_str}, ${some_char}");,
        Union::D{some_intseq} => println("${some_intseq}");
    }
}