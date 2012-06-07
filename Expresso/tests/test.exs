/***
 *	This is a test program for Expresso
 */
 
def GetInt()
{
	return 10;
}

def PrintInt($n)
{
	print $n;
}

def MakeList()
{
	return [1,2,3,4,5,6];
}

def MakeRange()
{
	return (1..5);
}

def main()
{
	let $x = 0, $y = 1;
	$x = $x + 5;
	let $z = $x * $y - 1;
	print $x, $y, $z;
	let $w (- int;
	$w, $y = $x, $z;
	let $str = "This is a test. blah blah...", $str2 = "これはテストです。あーあー";
	let $flag = true;
	PrintInt(100);
	print $x, $y, $z, $w;
	print $str, $str2;
	print $flag;
	if($flag){
		$w = GetInt();
		print $w;
	}
	while($x > 0){
		PrintInt($x);
		$x = $x - 1;
	}
	let $list_obj = MakeList();
	let $range_obj = MakeRange();
	for($x in $range_obj)
		print $x;
}
