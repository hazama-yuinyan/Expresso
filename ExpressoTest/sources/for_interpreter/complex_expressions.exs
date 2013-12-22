/***
 * Test program for complex(compound) expressions
 */



def main()
{
	let x = [{x for x in [0..100]}];
	let y = [{x for x in [0..100] if x % 2 == 0}];
	let z = [{(x, y) for x in [0..100] if x % 2 == 0 for y in [0..100]}];

	let a (- int, b (- int, c (- int;
	a = b = c = 0;

	let m = (1, 3);

	return [x, y, z, a, b, c, m];
}