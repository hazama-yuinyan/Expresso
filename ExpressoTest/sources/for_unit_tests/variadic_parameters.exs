module main;


def createList(a (- int, b (- int, rest... (- int[])
{
	return Enumerable.Concat([a, b], rest);
}

def main()
{
	let vec = createList(1, 2, 3, 4, 5);
	pritnln("${vec}");
}