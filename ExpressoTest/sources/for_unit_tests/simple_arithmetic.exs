/***
 * Test program for simple arithmetic operations.
 */
module main;


def main()
{
	var x = 3, xp = 3, xm = 3, xt = 3, xd = 4, xmod = 4, xpower = 3;
	let a = x + 4;
	let b = x - 4;
	let c = x * 4;
	let d = 4 / 2;
	let e = 4 % 2;
	let f = x ** 2;

	xp += 4;
	xm -= 4;
	xt *= 4;
	xd /= 2;
	xmod %= 2;
	xpower **= 2;

	return new [x, a, b, c, d, e, f, xp, xm, xt, xd, xmod, xpower];
}
