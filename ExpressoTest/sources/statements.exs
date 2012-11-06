/***
 * Test program for statements
 */


def main(){
	let x = 100, y = 200, z = 300, w = 400;
	let p (- int;
	let flag (- bool;

	print "(x, y, z, w) = ", x, y, z, w;

	if(x == 100)
		flag = true;
	else
		flag = false;

	let sum = 0;
	for(p in 0..100){
		sum = sum + p;
		print p, sum;
	}

	let tmp = "";
	let strs = ["akarichan", "chinatsu", "kyoko", "yui"];
	for(tmp in strs){
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

	return [x, y, z, w, flag, sum, strs];
}