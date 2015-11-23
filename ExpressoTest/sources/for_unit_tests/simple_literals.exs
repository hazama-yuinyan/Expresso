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
    let f_c2 = .000_1;
    let d = 10000000L;
    let d_ (- bigint = 10000000l;
    let d2 = 10_000_000L;
    let e = "This is a test";
    let u = 1000u;
    let u_ = 1000U;
    let f_a = 1.0e4f;
    let f_a_ (- float = 1.0e4f;
    let f (- int[] = [];
    let f_ = [1, 2, 3];
    let f2 (- vector<int> = [...];
    let f2_ = [1, 2, 3, ...];
    let f3 = ();
    let f3_ = (1, "abc", true);
    let g (- dictionary<string, int> = {};
    let g_ = {"akari" : 13, "chinatsu" : 13, "京子" : 14, "結衣" : 14};
    var h = "私変わっちゃうの・・？";
    var h2 = "Yes, you can!";
    var h3 = 'a';
    let i = "よかった。私変わらないんだね！・・え、変われないの？・・・なんだかフクザツ";
    let i2 = "Oh, you just can't...";
    let i3 = 'a';
    let i4 = "\u0041\u005a\u0061\U007A\u3042\u30A2";   //AZazあア
    let i5 = '\u0041';
    let i6 = "This is a normal string.\n Seems 2 lines? Yes, you're right!";
    let i6_ = r"This is a raw string.\n Seems 2 lines? Nah, indeed.";
    let j = 1..10;
    let j2 = 1..10:1;
    let j3 (- intseq = 1..10:1;
    let j_2 = -5..-10:-1;
    let j_2_ = 0...-10:-1;

    println(a, h_a, h_a_, b, f_b, f_b_, c, f_c, f_c2, d, d_, d2, e, u, u_, f_a, f_a_, f, f_, f2, f2_, f3, f3_, g, g_, h, h2, h3, i, i2, i3, i4, i5, i6, i6_, j, j2, j3, j_2, j_2_);
}