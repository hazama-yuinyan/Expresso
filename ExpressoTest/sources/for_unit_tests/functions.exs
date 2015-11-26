/***
 * Test program for function declarations and calls.
 */
module main;


def test()
{
	let a = 10;
	return a + 10;
}

def test2(n (- int)
{
	return n + 10;
}

def test3(n (- int) -> int
{
	return n + 20;
}

def main()
{
	let a = test();
	let b = test2(20);
	let c = test3(20);

	println(a, b, c);
}
