module main;


enum SomeEnum<T, U>
{
    A(T, U),
    B(U)
}

def main()
{
    let a = SomeEnum::A<int>{0: 10, 1: 20.0f};
}