/**
 * Test for module declaration and import
 */
module main;


import "./test_module.exs" as TestModule;


def main()
{
	let a = TestModule::TestClass{x : 100, y : 300};
	let b = TestModule::createTest(50, 100);
	let c = TestModule::pair;
    let d = TestModule::mySin(0.0);
	
	println(a, b, c, d);
}
