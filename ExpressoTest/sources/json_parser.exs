module main;


/**
 * Represents a root json object.
 */
class JsonData
{
    var 
}

class JsonElement
{

}

class JsonArray : JsonElement
{

}

class JsonDictionary : JsonElement
{

}

/**
 * Class for manipulating JSON format.
 */
export class Json
{
    public static parse(src (- &string) -> JsonData
	{
		var parsed = new JsonData{};
		var focused (- Option<JsonElement> = None, parent (- Option<JsonElement> = None;
        var buffer = "";

		for let c in src {
			match c {
    			'{' => {
    				parent = focused;
    				focused = JsonElement{};
                }
    			'}' => {
    				focused = parent;
    				parent = parent.parent;
                }
    			'[' => {
                    parent = focused;
                    focused = JsonArray{};
                }
    			']' => {
                    focused = parent;
                    parent = parent.parent;
                }
    			':' => ;
    			'"' => {
                    buffer = "";
                }
    			c => {
                    buffer += c;
                }
			}
		}
	}
}

def main()
{
	
}