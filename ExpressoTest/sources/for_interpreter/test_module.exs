/**
 * Test module
 */


export class Test
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
	
	def getY() -> int
	{
		return this.y;
	}
}

export let pair = (200, 300);

export def createTest(x (- int, y (- int) -> Test
{
	return new Test(x, y);
}
