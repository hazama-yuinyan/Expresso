module main;


def createList<T>(a (- T, b (- T, c (- T) -> vector<T>
{
    return [a, b, c, ...];
}


def main()
{
	let a = 1, b = 2, c = 3;
	let vec = createList(a, b, c);
    //TODO: createListの戻り値の型を置換するところを実装する
    println("${vec}");
}