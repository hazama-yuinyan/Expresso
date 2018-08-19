.PHONY: Expresso.sln

DEST_BIN = /usr/local/bin
DEST = /usr/local/lib/exsc
DEST_LIB = ~/lib
EXE_SH = ./exsc
EXE = bin/Release/exsc.exe
EXE_CONFIG = bin/Release/exsc.exe.config
EXPRESSO_DLL = bin/Release/Expresso.dll
EXPRESSO_RUNTIME_DLL = bin/Release/ExpressoRuntime.dll
NREFACTORY_DLL = Expresso/bin/Release/ICSharpCode.NRefactory.dll
COLLECTIONS_DLL = bin/Release/Microsoft.Experimental.Collections.dll
IMMUTABLE_DLL = bin/Release/System.Collections.Immutable.dll
METADATA_DLL = bin/Release/System.Reflection.Metadata.dll
SOLUTION = ./Expresso.sln

all: exsc.exe

exsc.exe: $(SOLUTION)
	nuget restore $(SOLUTION)
	msbuild $(SOLUTION) /p:Configuration=Release /p:Platform="x86"

install: $(EXE)
	install -d $(DEST)
	install -d $(DEST_LIB)
	install $(EXE) $(DEST)
	install $(EXPRESSO_DLL) $(DEST_LIB)
	install $(EXPRESSO_RUNTIME_DLL) $(DEST_LIB)
	install $(NREFACTORY_DLL) $(DEST)
	install $(COLLECTIONS_DLL) $(DEST)
	install $(IMMUTABLE_DLL) $(DEST)
	install $(METADATA_DLL) $(DEST)
	install $(EXE_CONFIG) $(DEST)
	install $(EXE_SH) $(DEST_BIN)
	echo "export MONO_PATH=~/lib" >> ~/.bashrc
