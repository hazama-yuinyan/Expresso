module main;


import InteroperabilityTest.{InteroperabilityTest, StaticTest} from "./InteroperabilityTest.dll" as {InteroperabilityTest, StaticTest};

def main()
{
	let t = InteroperabilityTest{};
	t.doSomething();
	let i = t.getSomeInt();
	let list = t.getIntList();

	StaticTest.doSomething();
	let flag = StaticTest.getSomeBool();
	let seq = StaticTest.getSomeIntSeq();

	println(i, list, flag, seq);
}