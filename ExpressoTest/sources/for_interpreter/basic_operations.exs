/***
 * Test program for basic operations
 */
module main;


def main()
{
	let a = new [1, 2, 3];
 	let b = new {"a" : 1, "b" : 4, "y" : 10};
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
    let w = a[0] + a[1] * b["a"];
    let y = v * w;

 	return [a, b, c, d, x, p, q, r, s, t, u, v, w, y];
 }
