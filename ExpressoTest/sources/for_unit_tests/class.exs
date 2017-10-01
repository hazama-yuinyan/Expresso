/***
 * Test program for class in Expresso
 */
module main;


class TestClass
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

    public def getZ() -> int
    {
        return self.z;
    }
}

def main()
{
	let a = TestClass{x : 1, y : 2};
	//let b = new Test{x : 1, y : 2};
	let c = a.getX();
	let d = a.getY();
	let e = a.getXPlus(100);
    let f = a.getZ();
    let g = a.x;
	
	printFormat("(a.x, a.y, a.z, x) = ({0}, {1}, {2}, {3})\n", c, d, f, g);
}
