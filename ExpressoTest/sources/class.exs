/***
 * Test program for class in Expresso
 */


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
}

def main(){
	let a = new Test(1, 3);
	let b = a.getX();
	let c = a.getY();
	
	print "(a.x, a.y) = ", b, c;

	return [a, b, c];
}
