/***
 * Test program for built-in objects in Expresso
 */
module main;


def main()
{
	let a = [1, 2, 3, 4, 5, 6, 7, 8];
	let b = {"akari" : 13, "chinatsu" : 13, "kyoko" : 14, "yui" : 14};
	let c = ("akarichan", "kawakawa", "chinatsuchan", 2424);

	let d = a[0..3]/*.collect()*/;
	var y (- vector<int> = [...];
	for let x in d/*b*/ {
		print(x);
		y.add(x);
	}
	//let e = c[2];
	
    println(a, b, c, d, /*e, */y);
}