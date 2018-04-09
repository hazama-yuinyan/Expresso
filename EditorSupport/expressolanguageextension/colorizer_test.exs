module main;


import System.{Math, Exception} as {Math, Exception};
import System.RegularExpressions.Regex as Regex;

export class TestClass
{
    let x (- int;

    public def getX()
    {
        return self.x;
    }
}

def throwException()
{
    throw Exception{message: "Some error occurred"};
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
    
    println(a, sin);
}