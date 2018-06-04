module main;


def callClosure(closure (- |int| -> int)
{
    closure(1);
}

def main()
{
    let c = |x| x + 1;
    callClosure(c);
}