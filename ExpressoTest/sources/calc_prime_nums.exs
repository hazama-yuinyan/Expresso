/***
 * This is a performance test for Expresso
 */
module main; 
 
import System.Math as Math;
 
def main()
{
	let n = 100000;
    var results (- vector<int> = [...];
	for let i in 2...n {
		let SQRT_I = (Math.Sqrt(i as double) + 1.0) as int;
		for let j in 2...SQRT_I {
			if j == SQRT_I {
				results.Add(i);
			} else {
                if i % j == 0 {
				    break;
                }
            }
		}
	}
	
	print("Results: ");
	for let j in results {
		print("${j}, ");
	}
}
