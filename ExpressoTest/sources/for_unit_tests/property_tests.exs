module main;


import InteroperabilityTest.PropertyTest from "./InteroperabilityTest.dll" as PropertyTest;

def main()
{
	println("before: ${PropertyTest.SomeProperty}");
	PropertyTest.SomeProperty = 100;
	println("after: ${PropertyTest.SomeProperty}");

    var inst = PropertyTest{};
    println("before: ${inst.SomeInstanceProperty}");
    inst.SomeInstanceProperty = 1000;
    println("after: ${inst.SomeInstanceProperty}");
}