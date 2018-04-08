/**
 * Test module
 */
module test_module;

import System.Math as Math;


export class TestClass3
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

export def createTest(x (- int, y (- int) -> TestClass3
{
	return TestClass3{x : x, y : y};
}

export def mySin(x (- double) -> double
{
    return Math.Sin(x);
}
