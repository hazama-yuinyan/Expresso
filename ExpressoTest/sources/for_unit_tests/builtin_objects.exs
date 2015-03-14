/***
 * Test program for built-in objects in Expresso
 */
module main;


def main()
{
	let a = new [1, 2, 3, 4, 5, 6, 7, 8];
	let b = new {"akari" : 13, "chinatsu" : 13, "kyoko" : 14, "yui" : 14};
	let c = new ("akarichan", "kawakawa", "chinatsuchan", 2424);

	let d = a[0..3].collect();
	var y = [...];
	for let x in b {
		print(x);
		y.add(x);
	}
	let e = c[2];
	
	return new [a, b, c, d, e, x, y];
}