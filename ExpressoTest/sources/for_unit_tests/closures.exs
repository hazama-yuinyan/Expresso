/***
 * Test program for closures
 */
module main;


def addOneToOne(addOne (- |int| -> int)
{
    return addOne(1);
}

def main()
{
    let c = |x (- int| x + 1;
    let c2 = |x (- int| {
        let y = 1;
        return x + y;
    };
    let a = c(1);
    let b = c2(1);
    let d = addOneToOne(|x| x + 1);

    println(a, b, d);
}