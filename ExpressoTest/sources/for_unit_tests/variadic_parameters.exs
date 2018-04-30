module main;


def createList(a (- int, b (- int, rest... (- int[])
{
	var tmp_vec = [a, b, ...];
	for let item in rest {
		tmp_vec.Add(item);
	}

	return tmp_vec;
}

def main()
{
	let a = 1, b = 2;
	let vec = createList(a, b, 3, 4, 5);
	pritnln("${vec}");
}