/***
 * Test program for class in Expresso
 */

//require a as b;

class Test
{
private:
	x (- int, y (- int;

public:
	constructor(x, y)
	{
		this.x = x;
		this.y = y;
	}

	def getX()
	{
		return this.x;
	}

	def getY()
	{
		return this.y;
	}

	def getXPlus(n)
	{
		return this.x + n;
	}
}

def main(){
	let a = new Test(1, 3);
	let b = a.getX();
	let c = a.getY();
	let d = a.getXPlus(100);
	
	print "(a.x, a.y) = ", b, c;

	return [a, b, c, d];
}
