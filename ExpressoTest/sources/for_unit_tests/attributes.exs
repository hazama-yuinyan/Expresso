/**
 * Test program for attributes
 */
[assembly: AssemblyDiscription("test assembly for attributes")]

[CLSCompliant]
module main;

import System.{SerializableAttribute, ObsoleteAttribute, CLSCompliantAttribute} as {Serializable, Obsolete, CLSCompliant};
import System.Diagnostics.ConditionalAttribute as Conditional;
import System.Runtime.InteropServices.MarshalAsAttribute as MarshalAs;
import System.Reflection.AssemblyDescriptionAttribute as AssemblyDescription;



[Serializable]
class AttributeTest
{
	[Conditional("DEBUG")]
	let x (- int;

	[Obsolete]
	public def doSomething([MarshalAs(UnmanagedType.LPStr)] dummy (- string)
	{
		println("Do something");
	}

	[return: MarshalAs(UnmanagedType.I4)]
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
