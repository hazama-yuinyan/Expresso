/**
 * Test for module declaration and import
 */
module main;


import test_module from "./test_module.exs" as TestModule;


def main()
{
	let a = TestModule::TestClass3{x : 100, y : 300};
	let b = TestModule::createTest(50, 100);
	let c = TestModule::pair;
    let d = TestModule::mySin(0.0);
    let e = a.getX();
	
	println(a, b, c, d, e);
}
