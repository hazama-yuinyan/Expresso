/***
 * Test program for basic operations
 */


def main(){
	let a = [1, 2, 3];
 	let b = {"a" : 1, "b" : 4, "y" : 10};
 	let c = "akarichan";
 	let d = "chinatsu";
 	let x = 100;

 	let p = a[0] + a[1] + a[2];
 	let q = b["a"] + b["b"] + b["y"];
 	let r = x >> p;
 	let s = x << 2;
 	let t = r & s;
 	let u = x | t;
 	let v = c + d;

 	return [a, b, c, d, x, p, q, r, s, t, u, v];
 }