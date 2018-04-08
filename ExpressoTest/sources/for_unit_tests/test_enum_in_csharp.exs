module main;

import InteroperabilityTest.{EnumTest, TestEnum} from "./InteroperabilityTest.dll" as {EnumTest, TestEnum};


def main()
{
	if EnumTest.testEnumeration(TestEnum.SomeField) {
		println("matched!");
	}

	var tester = EnumTest{};
	tester.TestProperty = TestEnum.YetAnotherField;
	if tester.testEnumerationOnInstance() {
		println("matched again!");
	}
}