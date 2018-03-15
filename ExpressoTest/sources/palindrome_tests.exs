module main;


def isPalindrome(s (- string) -> bool
{
	var flag = true;
	for let ch1 in s {
		for let ch2 in s.reverse() {
			if ch1 != ch2 {
				flag = false;
				break upto 2;
			}
		}
	}

	return flag;
}

def main()
{
	let a = "racecar";
	let b = "noon";
	let c = "civic";
	let d = "radar";
	let e = "characters";

	println(isPalindrome(a));
	println(isPalindrome(b));
	println(isPalindrome(c));
	println(isPalindrome(d));
	println(isPalindrome(e));
}