/**
 * Test program for attributes
 */
#[asm: AssemblyDescription{description: "test assembly for attributes"}]

#[CLSCompliant{isCompliant: true}]
module main;

import System.{SerializableAttribute, CLSCompliantAttribute, ObsoleteAttribute, Attribute, AttributeUsageAttribute, AttributeTargets} as 
{SerializableAttribute, CLSCompliantAttribute, ObsoleteAttribute, Attribute, AttributeUsageAttribute, AttributeTargets};
import System.Diagnostics.ConditionalAttribute as ConditionalAttribute;
import System.Reflection.AssemblyDescriptionAttribute as AssemblyDescriptionAttribute;



#[AttributeUsage{validOn: AttributeTargets.All}]
class AuthorAttribute : Attribute
{
    let name (- string;
}

#[Author{name: "train12"}]
let y = 100;

#[Obsolete]
def doSomethingInModule()
{
    ;
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
	public def doSomething2(n = 100) -> int
	{
        println("${n}");
		return 10;
	}
}

def main()
{
    let x = AttributeTest{x: 10};
    x.doSomething("some string");
    x.doSomething2();
}
