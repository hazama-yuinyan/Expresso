/***
 * Test program for general expressions
 */
module main;

class Test
{
    public let x (- int;
    let y (- int, z = 3;
}

export def createTest(x (- int, y = 2, z (- int = 3) -> Test
{
    return new Test{x : x, y : y, z : z};
}

def main()
{
    let ary = [1, 2, 3];
    let d = {"a" : 14, "b" : 13, "何か" : 100};

    let k = Test{x : 1, y: 2};
    let k2 = new Test{x : 1, y: 2};
    let l = createTest(1, 2, 4);
    let m = ary[0];
    let m2 = d["a"];
    let n = k.x;
    let n2 = k.getY();
    let x = 100;
    let p = ary[0] + ary[1] + ary[2];
    let q = d["a"] + d["b"] + d["何か"];
    let r = x >> p;
    let s = x << 2;
    let t = r & s;
    let v = x | t;
    let w = c + d;
    let y = ary[0] + ary[1] * d["a"];
    let z = v * w;
}
