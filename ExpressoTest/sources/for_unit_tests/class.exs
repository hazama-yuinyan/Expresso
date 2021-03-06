/***
 * Test program for class in Expresso
 */
module main;


class TestClass
{
	public let x (- int;
	let y (- int, z (- int;

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
	let a = TestClass{x : 1, y : 2, z : 3};
	//let b = new Test{x : 1, y : 2};
	let c = a.getX();
	let d = a.getY();
	let e = a.getXPlus(100);
    let f = a.getZ();
    let g = a.x;
	
	println("(a.x, a.y, a.z, x) = (${c}, ${d}, ${f}, ${g})");
}
