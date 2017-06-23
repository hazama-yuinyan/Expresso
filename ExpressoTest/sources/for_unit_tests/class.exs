/***
 * Test program for class in Expresso
 */
module main;


class Test
{
	public let x (- int;
	let y (- int, z = 3;

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
		return self.getX() + n;
	}
}

def main()
{
	let a = Test{x : 1, y : 3};
	let b = new Test{x : 1, y : 3};
	let c = a.getX();
	let d = a.getY();
	let e = a.getXPlus(100);
	
	printFormat("(a.x, a.y) = ({0}, {1})\n", c, d);
}
