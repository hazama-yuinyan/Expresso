module main;


/**
 * Represents a root json object.
 */
class JsonData
{
private:
	
}

class JsonElement
{

}

class JsonArray
{

}

/**
 * Class for manipulating JSON format.
 */
export class Json
{
private:
	

public:
	static parse(src (- string) -> JsonData
	{
		let parsed (- JsonData = new JsonData();
		let focused (- JsonElement = null, parent (- JsonElement = null;

		for(let c in src){
			switch(c){
			case '{':
				parent = focused;
				focused = new JsonElement();
			case '}':
				focused = parent;
				parent = parent.parent;
			case '[':
                parent = focused;
                focused = new JsonArray();
			case ']':
                focused = parent;
                parent = parent.parent;
			case ':':
                break;
			case '"':

			default:

			}
		}
	}
}

def main(args)
{
	
}