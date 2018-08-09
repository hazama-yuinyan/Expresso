module main;


class SomeClass
{
    public def doSomething()
    {
        println("DoSomething");
    }
}

def main()
{
    let x = SomeClass{};
    x.doSomething();
}