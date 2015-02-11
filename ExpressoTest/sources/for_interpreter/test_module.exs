/**
 * Test module
 */
module TestModule;


export class Test
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
}

export let pair = (200, 300);

export def createTest(x (- int, y (- int) -> Test
{
	return new Test{x : x, y : y};
}
