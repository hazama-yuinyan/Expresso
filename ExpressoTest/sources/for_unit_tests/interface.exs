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

class TestClass : IInterface
{
	let x (- int;

	def doSomeBehavior() -> int
	{
	    return self.x;
	}
}

def main()
{
	let t = TestClass{x: 1};
	let a = t.doSomeBehavior();
    printFormat("t.x = {0}", a);
}