/***
 * Test program for built-in objects in Expresso
 */
module main;


def main()
{
	let a = [1, 2, 3, 4, 5, 6, 7, 8];
	let b = {"akari" : 13, "chinatsu" : 13, "kyoko" : 14, "yui" : 14};
	let c = ("akarichan", "kawakawa", "chinatsuchan", 2424);

	let d = a[0..3];
    let e = [0..10];
    let f = [0..10, ...];
    let g = [0..-10:-1];
    let h = [0...100:2];
    let i = [5..15:2];
	var y (- vector<int> = [...];
	for let x in d {
		print("${x}");
		y.Add(x);
	}
	//let e = c[2];
	
    println("${a}, ${b}, ${c}, ${d}, ${e}, ${f}, ${g}");
    println("${h}, ${i}, ${y}");
}