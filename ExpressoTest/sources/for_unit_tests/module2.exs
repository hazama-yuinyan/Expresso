module main;


import test_module2::{TestClass4, createTest} from "./test_module2.exs" as {TestClass4, createTest};

def main()
{
	let t = TestClass4{x: 1, y: 2};
	let t2 = createTest(3, 4);

	println(t, t2);
}