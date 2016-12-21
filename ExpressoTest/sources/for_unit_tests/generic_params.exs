module main;


def createList<T>(a (- T, b (- T, rest... (- T[]) -> vector<T>
{
    return [a, b, ...].join(rest);
}


def main()
{
	let a = 1, b = 2, c = 3;
	let vec = createList(a, b, c);
}