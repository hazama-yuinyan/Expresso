.PHONY: Expresso.sln

DEST_BIN = /usr/bin
DEST = /usr/lib
EXE_SH = ./exsc
EXE = bin/Release/exsc.exe
EXPRESSO_DLL = bin/Release/Expresso.dll
EXPRESSO_RUNTIME_DLL = bin/Release/ExpressoRuntime.dll
NREFACTORY_DLL = bin/Release/ICSharpCode.NRefactory.dll
COLLECTIONS_DLL = bin/Release/Microsoft.Experimental.Collections.dll
IMMUTABLE_DLL = bin/Release/System.Collections.Immutable.dll
METADATA_DLL = bin/Release/Sysmtem.Reflection.Metadata.dll

all: exsc.exe

exsc.exe: Expresso.sln
	nuget restore
	msbuild Expresso.sln /p:Configuration=Release /p:Platform="x86"

install: $(EXE)
	install -s $(EXE) $(DEST)
	install -s $(EXPRESSO_DLL) $(DEST)
	install -s $(EXPRESSO_RUNTIME_DLL) $(DEST)
	install -s $(NREFACTORY_DLL) $(DEST)
	install -s $(COLLECTIONS_DLL) $(DEST)
	install -s $(IMMUTABLE_DLL) $(DEST)
	install -s $(METADATA_DLL) $(DEST)
	install -s $(EXE_SH) $(DEST_BIN)
