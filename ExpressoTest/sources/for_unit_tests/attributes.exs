/**
 * Test program for attributes
 */
#[assembly: AssemblyDescription{description: "test assembly for attributes"}]

#[Author{name: "train12"}]
module main;

import System.{SerializableAttribute, ObsoleteAttribute, Attribute, AttributeUsageAttribute, AttributeTargets} as
{SerializableAttribute, ObsoleteAttribute, Attribute, AttributeUsageAttribute, AttributeTargets};
import System.Diagnostics.ConditionalAttribute as ConditionalAttribute;
import System.Reflection.AssemblyDescriptionAttribute as AssemblyDescriptionAttribute;



#[AttributeUsage{validOn: AttributeTargets.All}]
class AuthorAttribute : Attribute
{
    let name (- string;
}

#[Serializable]
class AttributeTest
{
	#[Conditional{conditionString: "DEBUG"}]
	let x (- int;

	#[Obsolete]
	public def doSomething(#[Author{name: "train12"}] dummy (- string)
	{
		println("Do something");
	}

	#[return: Author{name: "train12"}]
	public def doSomething2() -> int
	{
		return 10;
	}
}

def main()
{
    let x = AttributeTest{x: 10};
    x.doSomething("some string");
    x.doSomething2();
}
