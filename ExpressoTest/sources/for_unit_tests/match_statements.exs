module main;


class TestClass
{
    public let x (- int, y (- int, z (- int;
}

def main()
{
	let tmp = "akarichan";
	match tmp {
		"akarichan" => print("kawakawa");,
		"chinatsu" if true => print("ankokuthunder!");,
        "kyoko" => print("gaichiban!");,
    	"yui" => print("doyaxtu!");
	}

    let tmp2 = 1;
    match tmp2 {
        0 => print("0");,
        1 | 2 => print("1 or 2");,
        x @ 3...10 => printFormat("{0} is in the range of 3 to 10", x);,
        _ => print("otherwise");
    }

    let tmp3 = TestClass{x: 1, y: 2, z: 3};
    match tmp3 {
        TestClass{x, ..} => printFormat("x is {0}", x);,
        TestClass{x, y, _} => printFormat("x is {0} and y is {1}", x, y);
    }

    let tmp4 = [1, 2, 3, 4, ...];
    match tmp4 {
        [x, y, .., ...] => printFormat("x and y are both vector's elements and the values are {0} and {1} respectively", x, y);,
        [1, 2, 3, _, ...] => print("tmp4 is a vector");
    }

    let tmp5 = (1, 2);
    match tmp5 {
        (x, ..) => printFormat("x is {0}", x);,
        (x, y) => printFormat("x is {0} and y is {1}", x, y);
    }
}