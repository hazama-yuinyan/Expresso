module main;


import InteroperabilityTest.{InteroperabilityTest, StaticTest} from "./InteroperabilityTest.dll" as {InteroperabilityTest, StaticTest};

def main()
{
	let t = InteroperabilityTest{};
	t.DoSomething();
	let i = t.GetSomeInt();
	let list = t.GetIntList();

	StaticTest.DoSomething();
	let flag = StaticTest.GetSomeBool();
	let seq = StaticTest.GetSomeIntSeq();

	println("${i}, ${list}, ${flag}, ${seq}");
}