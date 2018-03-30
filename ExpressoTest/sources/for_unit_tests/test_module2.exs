/**
 * Test module
 */
module test_module2;

import System.Math as Math;


export class TestClass4
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

export def createTest(x (- int, y (- int) -> TestClass4
{
	return TestClass4{x : x, y : y};
}

export def mySin(x (- double) -> double
{
    return Math.sin(x);
}
