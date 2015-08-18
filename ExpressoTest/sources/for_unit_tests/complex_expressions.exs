/***
 * Test program for complex(compound) expressions
 */
module main;

def createList<T>(a (- T, b (- T, rest... (- T[]) -> vector<T>
{
    return [a, b, ...].join(rest);
}

def main()
{
	let x = [x for x in 0..100];
	let y = [x for x in 0..100 if x % 2 == 0];
	let z = [(x, y) for x in 0..100 if x % 2 == 0 for y in 0..100];
    let triangles = [(a, b, c) for c in 1...10 for b in 1...c for a in 1...b if a ** 2 + b ** 2 == c ** 2];
    let specific_triangles = [(a, b, c) for c in 1...10 for b in 1...c for a in 1...b if a ** 2 + b ** 2 == c ** 2 && a + b + c == 24];

	var a (- int, b (- int, c (- int;
	a = b = c = 0;
    a, b, c = 1, 2, 3;

	return [x, y, z, specific_triangles, a, b, c];
}