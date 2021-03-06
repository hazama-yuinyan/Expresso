/**
 * Test program for an interface
 */
module main;



// interface IInterface
// {
//     doSomeBehavior() -> int;
// }
// I wonder if we can write like above
interface IInterface
{
	def doSomeBehavior() -> int;
}

class TestClass2 : IInterface
{
	let x (- int;

	public def doSomeBehavior() -> int
	{
	    return self.x;
	}
}

def main()
{
	let t = TestClass2{x: 1};
	let a = t.doSomeBehavior();
    println("t.x = ${a}");
}