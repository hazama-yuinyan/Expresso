module main;


class TestClass5
{
	let x (- int;
	public let y (- int;

	public def getX()
	{
		return self.x;
	}
}

def main()
{
	let x = 5;
	let t = TestClass5{x: 1, y: 2};
	let ary = [1, 1, 2, 3, 5, 8];

	let a = "some string";
	let b = "some string containing templates: ${x + 1}";
	let c = "another string containing templates: ${t.getX()}, ${t.y}";
	let d = "the 6th fibonacci number is ${ary[5]}";
    let e = "a string containing dollar symbol: $$x = ${x}";

	println("${a} ${b} ${c} ${d} ${e}");
}