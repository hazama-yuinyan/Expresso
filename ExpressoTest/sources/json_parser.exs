module main;


/**
 * Represents a root json object.
 */
class JsonData
{
private:
	
}
<?php
    $user_name = "user";
    $password = "hikar!729";
    $db_name = "crossroad";
    $connection = mysqli_connect("localhost", $user_name, $password);
    if(!$connection){
        print(mysqli_error());
        mysqli_close($connection);
    }
?>
<!DOCTYPE html>
<html>
    <head>
        <meta charset="utf-8">
        <title>Database test</title>
    </head>
    <body>
        <div>ステータス：<?php echo $connection ? "接続確立" : "接続失敗"; ?></div>
        <form id="db.php">
            <input name="id" type="text" placeholder="id" />
            <input name="num" type="text" placeholder="num" />
            <input name="str" type="text" placeholder="str" />
            <input name="created_at" type="date" />
            <input name="deleted_at" type="date" />
            <input type="button" value="セット" />
        </form>
        <div>
            <script type="text/javascript">
                function retrieveDatabase(){
                    var id_selector = document.getElementById("idSelector");
                    var id = id_selector.value;

                    var xhr = new XMLHttpRequest();
                    xhr.onload = function(res){
                        var response_text = res.responseText;
                        var display_elem = document.getElementById("responseDisplay");
                        //var json_obj = JSON.parse(response_text);
                        display_elem.innerHtml = response_text.replace("\n", "<br>");
                    };
                    xhr.open("post", "get_db.php", false);
                    xhr.send({id : id});
                }
            </script>
            <p id="responseDisplay"></p>
            <p>
                <input id="idSelector" type="text" placeholder="enter id to retrieve from database" />
                <button id="retrieveButton" onclick="retrieveDatabase();">表示</button>
            </p>
        </div>
    </body>
</html>
<?php mysqli_close($connection); ?>
class JsonElement
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

			case ']':

			case ':':

			case '"':

			default:

			}
		}
	}
}

def main(args)
{
	
}