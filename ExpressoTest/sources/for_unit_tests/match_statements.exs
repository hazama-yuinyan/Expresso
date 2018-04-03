/**
 * Test program for match statements in Expresso.
 */
module main;


class TestClass6
{
    public let x (- int, y (- int, z (- int;
}

def main()
{
	let tmp = "akarichan";
	match tmp {
		"akarichan" => println("kawakawa");,
		"chinatsu" if true => println("ankokuthunder!");,
        "kyoko" => println("gaichiban!");,
    	"yui" => println("doyaxtu!");
	}

    let tmp2 = 1;
    match tmp2 {
        0 => println("0");,
        1 | 2 => println("1 or 2");,
        i @ 3...10 => println("${i} is in the range of 3 to 10");,
        _ => println("otherwise");
    }

    let tmp3 = TestClass6{x: 1, y: 2, z: 3};
    match tmp3 {
        TestClass6{x, ..} => println("x is ${x}");,
        TestClass6{x, y, _} => println("x is ${x} and y is ${y}");
    }

    /*let tmp4 = [1, 2, 3, 4, ...];
    match tmp4 {
        [x, y, .., ...] => println("x and y are both vector's elements and the values are ${x} and ${y} respectively");,
        [1, 2, 3, _, ...] => print("tmp4 is a vector");
    }

    let tmp5 = [1, 2, 3, 4];
    match tmp5 {
        [x, y, ..] => println("x and y are both array's elements and the values are ${x} and ${y} respectively");,
        [1, 2, 3, _] => print("tmp5 is an array");
    }*/

    let tmp6 = (1, 2);
    match tmp6 {
        (x, ..) => println("x is ${x}");,
        (x, y) => println("x is ${x} and y is ${y}");
    }
}