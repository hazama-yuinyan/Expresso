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

def main()
{
	let $x = 0, $y = 1;
	$x = $x + 5;
	let $z = $x * $y - 1;
	print $x;
	print $y;
	print $z;
	let $w (- int;
	$w, $y = $x, $z;
	let $str = "This is a test. blah blah...", $str2 = "これはテストです。あーあー";
	let $flag = true;
	Print100();
	print $x;
	print $y;
	print $z;
	print $w;
	print $str;
	print $str2;
	print $flag;
	if($flag){
		$w = GetInt();
		print $w;
	}
	while($x > 0){
		Print100();
		$x = $x - 1;
	}
}
