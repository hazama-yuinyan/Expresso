/***
 * Test program for statements
 */
module main;


def main()
{
	let x = 100, y = 200, z = 300, w = 400;
	var flag (- bool;

	print("(x, y, z, w) = ", x, y, z, w);

	if x == 100
		flag = true;
	else
		flag = false;

	var sum = 0;
	for p in 0..y {
		sum += p;
		print p, sum;
	}

	let strs = ["akarichan", "chinatsu", "kyoko", "yui"];
	for tmp in strs {
		match tmp {
		"akarichan" => print("kawakawa");
		"chinatsu" => print("ankokuthunder!");
        "kyoko" => "gaichiban!";
        "yui" => "doyaxtu!";
		}
	}

	var fibs = [], a = 0, b = 1;
	while b < 1000 {
		fibs.add(b);
		a, b = b, a + b;
	}

	var ary = [];
	for i in 0..10 {
		for j in 0..10 {
			if i == 3 || i == 6{break;}
			if j == 8 {continue upto 2;}
			ary.add((i, j));
		}
	}

	return [x, y, z, w, flag, sum, strs, fibs, ary];
}