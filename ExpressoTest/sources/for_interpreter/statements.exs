/***
 * Test program for statements
 */


def main(){
	let x = 100, y = 200, z = 300, w = 400;
	let flag (- bool;

	print "(x, y, z, w) = ", x, y, z, w;

	if(x == 100)
		flag = true;
	else
		flag = false;

	let sum = 0;
	for(let p in [0..y]){
		sum += p;
		print p, sum;
	}

	let strs = ["akarichan", "chinatsu", "kyoko", "yui"];
	for(let tmp in strs){
		switch(tmp){
		case "akarichan":
			print "kawakawa";

		case "chinatsu":
			print "ankokuthunder!";

		case "kyoko":
			print "gaichiban!";

		case "yui":
			print "doyaxtu!";
		}
	}

	let fibs = [], a = 0, b = 1;
	while(b < 1000){
		fibs.add(b);
		a, b = b, a + b;
	}

	return [x, y, z, w, flag, sum, strs, fibs];
}