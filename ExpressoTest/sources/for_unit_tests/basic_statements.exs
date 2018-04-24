/***
 * Test program for statements
 */
module main;


def main()
{
	let x = 100, y = 20, z = 300, w = 400;
	var flag (- bool;

	print(x, y, z, w);

	if x == 100 {
		flag = true;
	}else{
		flag = false;
    }

	var sum = 0;
	for let p in 0..y {
		sum += p;
		println(p, sum);
	}

	var fibs (- vector<int> = [...], a = 0, b = 1;
	while b < 1000 {
		fibs.Add(b);
		a, b = b, a + b;
	}

    var n = 100;
    do{
        n -= 40;
        println(n);
    }while n > 0;

	var vec (- vector<int> = [...];
	for let i in [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, ...] {
        let MAX_J = 8;
		for let j in 0..10 {
			if i == 3 || i == 6 {break;}
			if j == MAX_J {continue upto 2;}
			vec.Add(i);
			vec.Add(j);
			println(i, j);
		}
	}

	println(flag, sum, a, b, fibs, vec);
}