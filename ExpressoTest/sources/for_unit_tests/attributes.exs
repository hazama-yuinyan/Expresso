/**
 * Test program for attributes
 */
#[assembly: AssemblyDescription{description: "test assembly for attributes"}]

#[CLSCompliant{isCompliant: true}]
module main;

import System.{SerializableAttribute, CLSCompliantAttribute, ObsoleteAttribute} as {SerializableAttribute, CLSCompliantAttribute, ObsoleteAttribute};
import System.Diagnostics.ConditionalAttribute as ConditionalAttribute;
import System.Reflection.AssemblyDescriptionAttribute as AssemblyDescriptionAttribute;
import test_attribute::AuthorAttribute from "./test_attribute.exs" as AuthorAttribute;



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

    //let attribute = AuthorAttribute{name: "foo"};
    /*let type_info = x.GetType();
    let asm = type_info.Assembly;
    let attribute1 = asm.GetCustomAttribute<AssemblyDescriptionAttribute>();
    println("${attribute1 != null}");

    let mod = type_info.Module;
    let attribute2 = mod.GetCustomAttribute<CLSCompliantAttribute>();
    println("${attribute2 != null}");*/
}
