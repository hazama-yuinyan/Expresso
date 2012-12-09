


def main(){
	let a = 0, b = 1;
	while(b < 10000){
		print b;
		a, b = b, a + b;
	}
}