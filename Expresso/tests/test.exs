/***
 *	This is a test program for Expresso
 */
 
def GetInt()
{
	return 10;
}

def Print100()
{
	print 100;
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
	Print100();
	print $x, $y, $z, $w;
	print $str, $str2;
	print $flag;
	if($flag){
		$w = GetInt();
		print $w;
	}
	while($x > 0){
		Print100();
		$x = $x - 1;
	}
	let $list1 = MakeList();
	let $range = MakeRange();
}
