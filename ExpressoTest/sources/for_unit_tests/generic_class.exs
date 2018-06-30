module main;


interface SomeInterface<T>
{
    def getX() -> T;
}

class GenericClass<T> : SomeInterface<T>
{
    let x (- T;

    public def getX() -> T
    {
        return self.x;
    }
}

enum MyOption<T>
{
    Ok(T),
    Error()
}

def main()
{
    let a = GenericClass<int>{x: 10};
    let b = GenericClass{x: 20};
    let c = MyOption::Ok<T = int>{0: 10};
    let d = MyOption::Ok{0: 20};

    println("${a}, ${b}, ${c}, ${d}");
}