


class JsonData
{
private:
	
}

class Json
{
private:
	

public:
	static parse(src (- string) -> JsonData
	{
		let parsed (- JsonData = new JsonData();
		let focused (- JsonElement = null, parent (- JsonElement = null;

		for(let c in src){
			switch(c){
			case "{":
				parent = focused;
				focused = new JsonElement();
			case "}":

			case "[":

			case "]":

			default:

			}
		}
	}
}

def main(args){
	
}