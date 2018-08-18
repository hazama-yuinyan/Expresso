.PHONY: Expresso.sln

DEST_BIN = /usr/bin
DEST = /usr/lib/exsc
EXE_SH = ./exsc
EXE = bin/Release/exsc.exe
EXE_CONFIG = bin/Release/exsc.exe.config
EXPRESSO_DLL = bin/Release/Expresso.dll
EXPRESSO_RUNTIME_DLL = bin/Release/ExpressoRuntime.dll
NREFACTORY_DLL = Expresso/bin/Release/ICSharpCode.NRefactory.dll
COLLECTIONS_DLL = bin/Release/Microsoft.Experimental.Collections.dll
IMMUTABLE_DLL = bin/Release/System.Collections.Immutable.dll
METADATA_DLL = bin/Release/Sysmtem.Reflection.Metadata.dll
SOLUTION = ./Expresso.sln

all: exsc.exe

exsc.exe: $(SOLUTION)
	nuget restore $(SOLUTION)
	msbuild $(SOLUTION) /p:Configuration=Release /p:Platform="x86"

install: $(EXE)
	install -d $(EXE) $(DEST)
	install -d $(EXPRESSO_DLL) $(DEST)
	install -d $(EXPRESSO_RUNTIME_DLL) $(DEST)
	install -d $(NREFACTORY_DLL) $(DEST)
	install -d $(COLLECTIONS_DLL) $(DEST)
	install -d $(IMMUTABLE_DLL) $(DEST)
	install -d $(METADATA_DLL) $(DEST)
	install -d $(EXE_CONFIG) $(DEST)
	install $(EXE_SH) $(DEST_BIN)
