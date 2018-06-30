module main;


enum SomeEnum<T>
{
    A(T)
}

def main()
{
    let a = SomeEnum::A<int>{0: 10.0};
}