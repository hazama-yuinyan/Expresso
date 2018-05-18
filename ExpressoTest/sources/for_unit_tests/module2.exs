module main;


import test_module2::{TestClass4, createTest, pair} from "./test_module2.exs" as {TestClass4, createTest, pair};

def main()
{
	let t = TestClass4{x: 1, y: 2};
	let t2 = createTest(3, 4);
    let x = pair;

	println("${t}, ${t2}, ${x}");
}