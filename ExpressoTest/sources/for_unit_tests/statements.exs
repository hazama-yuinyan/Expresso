/***
 * Test program for statements
 */
module main;


def main()
{
	let x = 100, y = 50, z = 300, w = 400;
	var flag (- bool;

	print("(x, y, z, w) = ({}, {}, {}, {})", x, y, z, w);

	if x == 100 {
		flag = true;
	}else{
		flag = false;
    }

	var sum = 0;
	for let p in 0..y {
		sum += p;
		println("{}, {}", p, sum);
	}

	let tmp = "akarichan";
	match tmp {
		"akarichan" => print("kawakawa");
		"chinatsu" if tmp.Length == 8 => print("ankokuthunder!");
        "kyoko" => print("gaichiban!");
    	"yui" => print("doyaxtu!");
	}

	var fibs = [...], a = 0, b = 1;
	while b < 1000 {
		fibs.add(b);
		a, b = b, a + b;
	}

	var vec = [...];
	for let i in 0..10 {
		for let j in 0..10 {
			if i == 3 || i == 6 {break;}
			if j == 8 {continue upto 2;}
			vec.add((i, j));
		}
	}

	return [x, y, z, w, flag, sum, strs, fibs, vec];
}