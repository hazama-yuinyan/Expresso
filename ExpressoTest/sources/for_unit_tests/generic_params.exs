module main;


def createList<T>(a (- T, b (- T, rest... (- T[]) -> vector<T>
{
    var tmp_vec = [a, b, ...];
    for let tmp in rest {
        tmp_vec.add(tmp);
    }
    return tmp_vec;
}


def main()
{
	let a = 1, b = 2, c = 3;
	let vec = createList(a, b, c);
    //TODO: createListの戻り値の型を置換するところを実装する
    println(vec);
}