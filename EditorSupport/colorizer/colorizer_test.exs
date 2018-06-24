module main;


import System.{Math, Exception, SerializableAttribute} as {Math, Exception, SerializableAttribute};
import System.Text.RegularExpressions.Regex as Regex;
import test_module::TestClass from "./testmodule.exs" as TestClass;
import OtherLanguageWorld.{StaticClass, TestEnum} from "./InteroperabilityTest.dll" as {StaticClass, TestEnum};

enum SomeEnum
{
    A(int),
    B(int, uint),
    C(SomeDerivedClass)
    
    public def print()
    {
        match self {
            SomeEnum::A{some_int} => println("${some_int}");,
            SomeEnum::B{some_int, some_uint} => println("${some_int} ${some_uint}");,
            SomeEnum::C{derived_class} => println("${derived_class}");
        }
    }

    public def printUsingIf()
    {
        if self == SomeEnum::A{some_int} {
            println("${some_int}");
        }
    }
}

enum SomeEnum2
{
    A = 1,
    B,
    C

    public def print()
    {
        match self {
            SomeEnum2::A => println("A");,
            SomeEnum2::B => println("B");,
            SomeEnum2::C => println("C");
        }
    }
}

enum SomeEnum3<T, U>
{
    A(int),
    B(uint, float)
}

export interface SomeInterface<T>
{
    def getX() -> T;
}

#[Serializable]
export class TestClass
{
    let x (- int;
    private let y (- SomeDerivedClass;

    #[Author{name: "train12"}]
    public def getX(){
        return self.x;
    }

    public def getY() -> SomeDerivedClass
    {
        return self.y;
    }
}

class SomeDerivedClass<T> : IInterface, IInterface2, TestClass
{
    let x (- T;
}

class SomeDerivedClass2: IInterface, IInterface2{
}

def throwException()
{
    throw Exception{message: "Some error occurred"};
}

def getInt() -> int
{
    return 10;
}

def returnInt(n (- int, j (- SomeDerivedClass) -> int
{
    return n;
}

def returnNull(str = "abc") -> SomeDerivedClass
{
    return null;
}

def returnIntSeq()
{
    return 0..10;
}

def main()
{
    let a = 100;
    // I'm a line comment
    /* I'm a multi-line comment
    blah blah
    */
    let sin = Math.Sin(0.0);
    let sin2 (- double = Math.Sin(0.0);
    let str = "\\tsome stringあいうえお日本語";
    let chr = 'a';
    let raw_string = r"some raw stringあいうえお日本語
    blah blah";
    let interpolation = "$$sin: ${sin}あいうえお日本語";
    let something = self.x;
    let b = 10, c = 20, d = 30;
    let e = getInt();
    let f = returnInt(10, 20);
    let g = e as double;

    var a2 = 100;
    var sin = Math.Sin(0.0);
    var str = "\\tsome stringあいうえお日本語";
    var chr = 'a';
    var raw_string = r"some raw string${sin}あいうえお日本語
    blah blah";
    var interpolation = "$$sin: ${sin}${getInt()}あいうえお日本語";
    var something = self.x;
    var b2 = 10, c2 = 20, d2 = 30;
    var e = getInt();
    var f = returnInt(10, 20);
    var g = e as double;

    for let (i, j) in (returnIntSeq(), returnIntSeq()) {
        println(i);
        println(j);
    }

    let flag = true, another_flag = false;
    if flag {
        println("true");
    }else if another_flag {
        println("another_flag");
    }else{
        println("false");
    }
    
    println(a, sin);

    let str = "abc";
    match str {
        "abc" => println("abc");,
        "def" => println("def");
    }
}