module main;



def main()
{
	let dict = {"akari": 13, "chinatsu": 13, "kyoko": 14, "yui": 14};
	let a = [(1, 2), (3, 4), (5, 6)];
    let v = [(7, 8), (9, 10), (11, 12), ...];

	for let (key, value) in dict {
		println("${key}: ${value}, ");
	}

	for let (first, second) in a {
		println("(${first}, ${second}), ");
	}

    for let (first2, second2) in v {
        println("(${first2}, ${second2}), ");
    }
}