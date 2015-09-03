/***
 * This is a performance test for Expresso
 */
module main; 
 
import sqrt in math;
 
def main()
{
	let n = 100000;
    var results = [...];
	for let i in 2...n {
		let SQRT_I = (sqrt(i as float) + 1.0) as int;
		for let j in 2...SQRT_I {
			if j == SQRT_I {
				results.add(i);
			} else if i % j == 0 {
				break;
            }
		}
	}
	
	print("Results: ");
	for let j in results {
		print(j);
	}
}
