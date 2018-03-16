module main;


def main()
{
    let n = 10000;
	var a = 0, b = 1;
	while b < n {
		print(b);
		a, b = b, a + b;
	}
}