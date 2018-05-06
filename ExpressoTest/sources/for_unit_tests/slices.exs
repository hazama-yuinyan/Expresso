module main;


def main()
{
	let a = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
	let b = a[2...4];

	let c = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, ...];
	let d = c[2...4];

	println("${b}, ${d}");
}