/***
 * Test program for parsing Expresso
 */
module main;

def main()
{
    let a = 255;
    let h_a = 0xff;
    let h_a_ (- int = 0xff;
    let b = 1000.0;
    let f_b = 1.0e4;
    let f_b_ (- double = 1.0e4;
    let c = 0.001;
    let f_c = .1e-2;
    let d = 10000000L;
    let d_ (- bigint = 10000000l;
    let e = "This is a test";
    let u = 1000u;
    let u_ = 1000U;
    let f_a = 1.0e4f;
    let f_a_ (- float = 1.0e4f;
    let f = [];
    let f_ = [1, 2, 3];
    let f2 = [...];
    let f2_ = [1, 2, 3, ...];
    let f3 = ();
    let f3_ = (1, "abc");
    let g = {};
    let g_ = {"akari" : 13, "chinatsu" : 13, "京子" : 14, "結衣" : 14};
    var h = "私変わっちゃうの・・？";
    var h2 = "Yes, you can!";
    var h3 = 'a';
    let i = "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ";
    let i2 = "Oh, you just can't...";
    let i3 = 'a';
    let j = 1..10;
    let j2 = 1..10:1;
    let j3 (- intseq = 1..10:1;
    let j_2 = -5..-10:-1;
    let j_2_ = 0...-10:-1;
    let k = Test{x : 1, y: 2};
    let k2 = new Test{x : 1, y: 2};
    let l = CreateTest(1, 2);
    let m = f_[0];
    let m2 = g_["akari"];
    let n = k.x;
    let n2 = k.getY();
    let x = 100;
    let p = f_[0] + f_[1] + f_[2];
    let q = g_["akari"] + g_["chinatsu"] + g_["結衣"];
    let r = x >> p;
    let s = x << 2;
    let t = r & s;
    let v = x | t;
    let w = c + d;
    let y = f_[0] + f_[1] * g_["akari"];
    let z = v * w;
}