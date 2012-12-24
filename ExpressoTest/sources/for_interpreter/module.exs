/**
 * Test for module declaration and import
 */


require test_module as TestModule;


def main()
{
	let a = new TestModule.Test(100, 300);
	let b = TestModule.createTest(50, 100);
	let c = TestModule.pair;
	
	return [a, b, c];
}
