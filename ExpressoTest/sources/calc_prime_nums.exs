/***
 * This is a performance test for Expresso
 */
 
 
//require math;
 
def main(){
	let n = 100000, i = 2, results = [];
	for(i in [2..n + 1]){
		let SQRT_I = toInt(sqrt(i) + 1.0);
		for(j in [2..SQRT_I]){
			if(j == SQRT_I)
				results.add(i);
			else if(i % j == 0)
				break;
		}
	}
	
	print "Results: ";
	for(let j in results){
		print j;
	}
}
