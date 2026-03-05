BINARY      := sharpscan
PROJECT     := SharpScan.csproj
RUNTIME_LIN := linux-x64
RUNTIME_WIN := win-x64
OUT_LIN     := dist/linux
OUT_WIN     := dist/windows

.PHONY: all build release run publish publish-linux publish-windows clean help

## Default: build self-contained debug binary for Linux
all: build

## Build debug self-contained binary for the local platform (linux-x64)
build:
	dotnet build $(PROJECT) -c Debug -r $(RUNTIME_LIN)

## Build release self-contained binary for the local platform (linux-x64)
release:
	dotnet build $(PROJECT) -c Release -r $(RUNTIME_LIN)

## Run directly via dotnet run  (ARGS="<target> -p <ports> ...")
run:
	dotnet run --project $(PROJECT) -r $(RUNTIME_LIN) -- $(ARGS)

## Publish single self-contained executable for Linux → dist/linux/sharpscan
publish-linux:
	dotnet publish $(PROJECT) -c Release -r $(RUNTIME_LIN) -o $(OUT_LIN)
	@echo "→ $(OUT_LIN)/$(BINARY)"

## Publish single self-contained executable for Windows → dist/windows/sharpscan.exe
publish-windows:
	dotnet publish $(PROJECT) -c Release -r $(RUNTIME_WIN) -o $(OUT_WIN)
	@echo "→ $(OUT_WIN)/$(BINARY).exe"

## Publish self-contained executables for all platforms
publish: publish-linux publish-windows

## Remove all build artefacts
clean:
	rm -rf bin obj dist

## Show available targets
help:
	@grep -E '^##' Makefile | sed 's/## /  /'
