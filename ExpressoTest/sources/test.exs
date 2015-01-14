/***
 *	This is a test program for Expresso
 */
module main;

// You can omit type annotations in function declarations
def getInt()
{
	return 10;
}

// Of course, you can explicitly specify them.
def printInt(n (- int) -> ()    //The void type is the synonym for the unit type
{
	print(n);
}

def makeList() -> vector<int>
{
	return new vector{1,2,3,4,5,6};
}

def makeDict() -> dictionary<string, int>
{
	return new dictionary{"あかり" : 13, "京子" : 14, "結衣" : 14, "ちなつ" : 13};
}

def makeTuple() -> (string, string, int)
{
	return "あかりちゃん", "かわいいよ、あかりちゃん", 130; //A return statement containing a list of expressions must yield a tuple.
}

def printList<T>(input (- T, header = "")
{
	print(header);
	print("リストの中身は");
	for(tmp in input)
		print(tmp);
	
	print("です。");
}

def testSwitch<T>(input (- T)
{
	match input {
	    "abc" => print("Detects a string");
	    "あかりちゃん" | "akarichan" => print("あかりんかわかわ");
	    0..10 => print("The value is in the range of 0 to 10");
        _ => print("unknown value!");
	}
}

def main()
{
	let x = 0, y = 1;
	x += 5;
	let z = x * y - 1;
	println("(x, y, z) = {}, {}, {}", x, y, z);
	let w (- int;
	w, y = x, z;
	let str = "This is a test. blah blah...", str2 = "これはテストです。あーあー";
	let flag = true;
	printInt(100);
	println("(x, y, z, w) = {}, {}, {}, {}", x, y, z, w);
	print(str, str2);
	print(flag);
	if flag {
		w = getInt();
		print(w);
	}
	while x > 0 {
		printInt(x);
		x -= 1;
	}
	let list_obj = makeList();
	let dict_obj = makeDict();
	let tuple_obj = makeTuple(), tuple_obj2 = ("あかりちゃん", "ちなつちゃん", 2424);
	print("Print range object:");
	for(let x in 1..5)
		print(x);
	
	/*for(let item in dict_obj)
		print item;*/
	
	printList(list_obj);
	let list_obj2 = list_obj[0..2];
	printList(list_obj2, "Sliced list:");
	println("Left shift:{}", w << 1);
	println("x + 1 + 3 * 2 = {}", x + 1 + 3 * 2);
	testSwitch("abc");
	testSwitch(5);
	testSwitch(tuple_obj2[0]);
	testSwitch("akarichan");
	
	let comp = [x for x in 0..100];
	printList(comp, "Created using comprehension:");
}
