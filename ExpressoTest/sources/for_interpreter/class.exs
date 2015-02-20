/***
 * Test program for class in Expresso
 */
module main;


class Test
{
	let x (- int, y (- int;

	public def getX()
	{
		return self.x;
	}

	public def getY() -> int
	{
		return self.y;
	}

	public def getXPlus(n (- int) -> int
	{
		return self.x + n;
	}
}

def main()
{
	let a = new Test{x : 1, y : 3);
	let (b, c) = (a.getX(), a.getY());
	let d = a.getXPlus(100);
	
	print("(a.x, a.y) = {}, {}", b, c);

	return new [a, b, c, d];
}
