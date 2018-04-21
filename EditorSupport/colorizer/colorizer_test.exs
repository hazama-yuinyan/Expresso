module main;


import System.{Math, Exception} as {Math, Exception};
import System.Text.RegularExpressions.Regex as Regex;
import test_module::TestClass from "./testmodule.exs" as TestClass;
import OtherLanguageWorld.{StaticClass, TestEnum} from "./InteroperabilityTest.dll" as {StaticClass, TestEnum};

export class TestClass
{
    let x (- int;

    public def getX()
    {
        return self.x;
    }
}

class SomeDerivedClass : IInterface, IInterface2, TestClass
{
}

class SomeDerivedClass2: IInterface, IInterface2{
}

def throwException()
{
    throw Exception{message: "Some error occurred"};
}

def getInt()
{
    return 10;
}

def returnInt(n (- int, j (- int)
{
    return n;
}

def main()
{
    let a = 100;
    // I'm a line comment
    /* I'm a multi-line comment
    blah blah
    */
    let sin = Math.Sin(0.0);
    let str = "\\tsome stringあいうえお日本語";
    let chr = 'a';
    let raw_string = r"some raw stringあいうえお日本語
    blah blah";
    let interpolation = "$$sin: ${sin}あいうえお日本語";
    let something = self.x;
    let b = 10, c = 20, d = 30;
    let e = getInt();
    let f = returnInt(10, 20);

    var a2 = 100;
    var sin = Math.Sin(0.0);
    var str = "\\tsome stringあいうえお日本語";
    var chr = 'a';
    var raw_string = r"some raw stringあいうえお日本語
    blah blah";
    var interpolation = "$$sin: ${sin}あいうえお日本語";
    var something = self.x;
    var b2 = 10, c2 = 20, d2 = 30;
    var e = getInt();
    var f = returnInt(10, 20);
    
    println(a, sin);
}