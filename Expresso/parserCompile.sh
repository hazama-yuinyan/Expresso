#! /bin/bash

cd ~/Documents/MonodevelopSolutions/Expresso/Expresso

export PATH=/Library/Frameworks/Mono.framework/Versions/Current/bin:${PATH}
mono ./Coco Expresso.ATG -trace GJSP
