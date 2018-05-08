# Expresso
-----------------

Expresso primarily aims to be an educational programming language. As such, it has similar syntaxes to PHP, and Java.
The language name __Expresso__ is coined by combining __Espresso__ with __expressive__, which means that it's easy for programmers to write codes in it as you can say "I can write a code while I'm having a cup of Espresso" and that it's highly expressive. And as such, Expresso is designed to provide high readability and perfomance at once.

## Characteristics

* Object-oriented(not mandatory, but highly recommended)
* Module based
* Builtin support for many useful types such as bigint, intseq, function and slice
* No traditional for statements
* Type-strict 
* Having collections in the built-in types
* Type inference
* Runs on the CLR(Common Language Runtime)
* Complete interoperability with other .NET languages at IL code level

## How to compile

First, `git clone` the repository and run `git submodule update --init`. Next download and move Coco(a Parser Generator for C#). If you are on Mac, then you can skip this step since `git clone` fetches a repository including the Coco. In order to download Coco, go to [here](http://www.ssw.uni-linz.ac.at/Coco/) and download `Coco.exe` in "Coco/R for C#" section. Unzip the downloaded file and move Coco.exe file to the root directory of Expresso *project*(that is the subdirectory of the root directory). Note that we're generating the parser using a shell script. So if you are on Windows, you will also have to write a batch file or something for generating the parser. And make a directory named "test_executables" in Expresso/ExpressoTest. In addition to those, you now have to get some NuGet pacakages. If you are using an IDE, it will be automatically downloaded if configured so. Finally, build the InteroperabilityTest project and move the resulting DLL to /ExpressoTest/sources/for_unit_tests/. This DLL is needed to run some tests. Then you should be ready to compile the projects.   
I've checked that it runs on Windows. However, most of the tests happily run on Windows but some don't, umm...
Just open up the solution on VS or whatever IDE you're using and build the 'ExpressoConsole' project. Since this project contains the front end executable, you can compile Expresso's source codes using the `exsc` command after that. Then all you have to do is to execute the binary produced by the `exsc` command with `mono`.

## 日本語
---------------

Expressoは、主に教育用言語を志向して作られた言語です。教育用言語ではありますが、今日のプログラミング言語としてあるべき機能を色々取り入れています。
言語名の__Expresso__は、__Espresso__と__expressive__を組み合わせた造語です。エスプレッソ一杯飲む間にコードが書けるほどの簡潔性と、表現力の高さを志向した名前になっています。そのために、スピードよりもRustでいう、ergonomicsを重視した言語設計になっています。
ですが、他の.NET言語との連携など、本格的に使用するユーザにも受け入れられるような機能の充実も志向しています。

## 特徴

* オブジェクト指向(もちろん、関数型などの他のパラダイムも使用できます)
* モジュール構成
* bigint, intseq, sliceなどの便利な型を組み込みでサポート
* C由来のfor文はなし
* 型厳密
* コレクションも組み込み型
* 型推論
* CLRで動作
* ILコードレベルで他の.NET言語と完全なる相互運用性を提供

## コンパイル方法

まず、リポジトリを`git clone`し、`git submodule update --init`を実行してください。次にCoco(C#用のパーサジェネレータ)をダウンロードし、配置します。Macをお使いなら、この手順は飛ばすことができます(多分、Linuxも)。Cocoは[こちら](http://www.ssw.uni-linz.ac.at/Coco/)に行き、「Coco/R for C#」セクションから`Coco.exe`をダウンロードしてください。ダウンロードしたファイルを解凍し、Expressoプロジェクトのルートディレクトリに配置してください。パーサは、シェルスクリプトで生成しています。なので、Windows上では、バッチファイルか何かを生成しなければならないでしょう。もちろん、パーサを更新しないのなら不要です。それから、ExpressoTestプロジェクトのルートディレクトリに"test_executables"ディレクトリを生成してください。テストを実行するために、このディレクトリは必要になります。また、NuGetパッケージを取得する必要があります。IDEを使用しているのなら、標準で足りないパッケージはダウンロードされるはずです。最後に`InteroperabilityTest`プロジェクトをビルドし、出来上がったDLLを`ExpressoTest/sources/for_unit_tests`に配置してください。このDLLは、あるテストに必要です。これでコンパイルする準備が整いました。
Windows上でも動作することは確認済みです。しかし、インターフェイスなど一部の機能が動作しません。.NETとMonoの実装の違いによるもののようなので、諦めてください。
VSなどのIDEでソリューションを開き、'ExpressoConsole'プロジェクトをビルドしてください。このプロジェクトにExpressoコンパイラのフロントエンド実行ファイルが含まれています。ビルドしたら、`exsc`コマンドでコンパイルができます。使用方法は、`mono exsc.exe source_file -o output_directory -e executable_name`です。コンパイルしたファイルもmonoか.NETランタイムで実行する必要があります。`mono executable_name`などで実行できるはずです。

## 文法など

簡単な文法は、テストのソースをご覧ください。`ExpressoTest/sources`配下にあります。一部実装されていない記法を使用しているものもありますが、概ねこんな書き方ができるという参考になるはずです。書きかけ、かつ英語だけですが、チュートリアルもあります。`Expresso/Documentations`をご覧ください。
