/**
 * Test program for parsing Expresso
 */
module main;

def main()
{
    let a = 10;
    if(a == 100){
        print(a);
    }else{
        print("a is not 100");
    }

    switch(b){
    case 10:
        print("b is 10");

    case 20:
        print("b is 20");
    }

    for(let i in ary){
        print(i);
        print(ary[i]);
    }

    while(c > 0){
        c -= 10;
    }

    return 0;
}