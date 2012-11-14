/***
 * Test program for complex(compound) expressions
 */



def main(){
	let x = [x for x in [0..99]];
	let y = [x for x in [0..100] if x % 2 == 0];
	let z = [(x, y) for x in [0..100] if x % 2 == 0 for y in [0..99]];

	return [x, y, z];
}