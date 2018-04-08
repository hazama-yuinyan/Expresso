module main;

import InteroperabilityTest.{EnumTest, TestEnum} from "./InteroperabilityTest.dll" as {EnumTest, TestEnum};


def main()
{
	if EnumTest.TestEnumeration(TestEnum.SomeField) {
		println("matched!");
	}

	var tester = EnumTest{};
	tester.TestProperty = TestEnum.YetAnotherField;
	if tester.TestEnumerationOnInstance() {
		println("matched again!");
	}
}