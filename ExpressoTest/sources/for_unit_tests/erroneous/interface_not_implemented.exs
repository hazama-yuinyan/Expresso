module main;


interface IInterface
{
	def requireYouToImplement() -> int;
}

class ConcreteClass : IInterface
{
	let x (- int;

	def getX()
	{
		return self.x;
	}
}

def main()
{
	let c = ConcreteClass{x: 1};
}