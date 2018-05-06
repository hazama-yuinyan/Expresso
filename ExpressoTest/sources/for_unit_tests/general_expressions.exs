/***
 * Test program for general expressions
 */
module main;



def main()
{
    let ary = [1, 2, 3];
    let d = {"a" : 14, "b" : 13, "何か" : 100};

    let m = ary[0];
    let m2 = d["a"];
    let x = 100;
    let p = ary[0] + ary[1] + ary[2];
    let q = d["a"] + d["b"] + d["何か"];
    let r = x >> p;
    let s = x << 2;
    let t = r & s;
    let v = x | t;
    let w = r + s;
    let y = ary[0] + ary[1] * d["a"];
    let z = v * w;

    println("${ary}, ${d}, ${m}, ${m2}, ${x}, ${p}, ${q}, ${r}, ${s}, ${t}, ${v}, ${w}, ${y}, ${z}");
}
