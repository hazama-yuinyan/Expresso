/**
 * Test program for closures with compound statements
 */
module main;


def main()
{
	let c = |f (- bool| {
	    if f {
	        return 1;
	    }else{
	        return 0;
	    }
	};
	let a = c(true);

	let c2 = |i (- int| {
	    var result = 0;
	    for let j in 0..i {
	    	result += j;
	    }
	    return result;
	};
	let b = c2(10);

	let c3 = |i (- int| {
	    var j = i;
	    while j > 0 {
	        println(j);
	        j -= 1;
	    }
	    println("BOOM!");
	};
	c3(3);

	println(a, b);
}