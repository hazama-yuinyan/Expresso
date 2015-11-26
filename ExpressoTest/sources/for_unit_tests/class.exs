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
		return self.x + n;
	}
}

def main()
{
	let a = Test{x : 1, y : 3};
	let a = new Test{x : 1, y : 3);
	let b = a.getX();
	let c = a.getY();
	let d = a.getXPlus(100);
	
	print("(a.x, a.y) = {}, {}", b, c);
}
