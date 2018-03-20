module main;


import test_module::{TestClass, createTest} from "./test_module.exs" as {TestClass, createTest};

def main()
{
	let t = TestClass{x: 1, y: 2};
	let t2 = createTest(3, 4);

	println(t, t2);
}