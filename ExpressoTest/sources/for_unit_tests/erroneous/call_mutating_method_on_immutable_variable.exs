module main;


class TestClass
{
	var x (- int;

	mutating public def mutatingMethod()
	{
		self.x += 1;
	}
}

def main()
{
	let t = TestClass{x: 1};
	t.mutatingMethod();
}