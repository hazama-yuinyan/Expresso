/**
 * Test program for attributes
 */
#[assembly: AssemblyDescription{description: "test assembly for attributes"}]

#[CLSCompliant{isCompliant: true}]
module main;

import System.{SerializableAttribute, ObsoleteAttribute, CLSCompliantAttribute, Attribute, AttributeUsageAttribute, AttributeTargets} as
{SerializableAttribute, ObsoleteAttribute, CLSCompliantAttribute, Attribute, AttributeUsageAttribute, AttributeTargets};
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

    /*let type_info = x.GetType();
    let asm = type_info.Assembly;
    let attribute1 = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
    println("${attribute1 != null}");

    let mod = type_info.Module;
    let attribute2 = mod.GetCustomAttribute<CLSCompliantAttribute>();
    println("${attribute2 != null}");*/
}
