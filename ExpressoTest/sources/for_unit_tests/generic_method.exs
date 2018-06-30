module main;


class SomeClass<T>
where T: IEquatable<T>
{
    public def equals<U>(other (- U) -> bool
    where U: T, IEquatable<U>
    {
        return self.x.Equals(other.x as T);
    }
}

class BaseClass : IEquatable<BaseClass>
{
    protected let x (- int;

    public def Equals(other (- BaseClass) -> bool
    {
        return self.x == other.x;
    }
}

class DerivedClass : BaseClass, IEquatable<DerivedClass>
{
    public def Equals(other (- DerivedClass) -> bool
    {
        return self.x == other.x;
    }
}

def main()
{
    let a = BaseClass{x: 10};
    let b = DerivedClass{x: 10};
    println("${a.Equals(b)}");
}