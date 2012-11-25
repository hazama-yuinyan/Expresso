/***
 *	This is a test program for Expresso
 */
 
def GetInt()
{
	return 10;
}

def PrintInt(n)
{
	print n;
}

def MakeList()
{
	return [1,2,3,4,5,6];
}

def MakeDict()
{
	return {"あかり" : 13, "京子" : 14, "結衣" : 14, "ちなつ" : 13};
}

def MakeTuple()
{
	return "あかりちゃん", "かわいいよ、あかりちゃん", 130; //A return statement containing a list of expressions must yield a tuple.
}

def PrintList(input, header = "")
{
	print header;
	print "リストの中身は";
	for(let tmp in input)
		print tmp;
	
	print "です。";
}

def TestSwitch(input)
{
	switch(input){
	case "abc":
		print "Detects a string";
	
	case "あかりちゃん":
	case "akarichan":
		print "あかりんかわかわ";
		
	case [0..10]:
		print "The value is in the range of 0 to 10";
	}
}

def main()
{
	let x = 0, y = 1;
	x += 5;
	let z = x * y - 1;
	print "(x, y, z) = ", x, y, z;
	let w (- int;
	w, y = x, z;
	let str = "This is a test. blah blah...", str2 = "これはテストです。あーあー";
	let flag = true;
	PrintInt(100);
	print "(x, y, z, w) = ", x, y, z, w;
	print str, str2;
	print flag;
	if(flag){
		w = GetInt();
		print w;
	}
	while(x > 0){
		PrintInt(x);
		x -= 1;
	}
	let list_obj = MakeList();
	let dict_obj = MakeDict();
	let tuple_obj = MakeTuple(), tuple_obj2 = ("あかりちゃん", "ちなつちゃん", 2424);
	print "Print range object:";
	for(x in [1..5])
		print x;
	
	/*for(let item in dict_obj)
		print item;*/
	
	PrintList(list_obj);
	let list_obj2 = list_obj[[0..2]];
	PrintList(list_obj2, "Sliced list:");
	print "Left shift:", w << 1;
	print "x + 1 + 3 * 2 = ", x + 1 + 3 * 2;
	TestSwitch("abc");
	TestSwitch(5);
	TestSwitch(tuple_obj2[0]);
	TestSwitch("akarichan");
	
	let comp = [x for x in [0..100]];
	PrintList(comp, "Created using comprehension:");
}
