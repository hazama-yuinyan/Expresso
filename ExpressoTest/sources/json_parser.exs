module main;


/**
 * Represents a root json object.
 */
class JsonData
{
    private
	
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
    public static parse(src (- string) -> JsonData
	{
		let parsed (- JsonData = new JsonData();
		let focused (- Option<JsonElement> = None, parent (- Option<JsonElement> = None;

		for let c in src {
			match c {
			'{' => {
				parent = focused;
				focused = new JsonElement();
            }
			'}' => {
				focused = parent;
				parent = parent.parent;
            }
			'[' => {
                parent = focused;
                focused = new JsonArray();
            }
			']' => {
                focused = parent;
                parent = parent.parent;
            }
			':' => {
                break;
            }
			'"' => {
            }
			_ => break;
			}
		}
	}
}

def main()
{
	
}