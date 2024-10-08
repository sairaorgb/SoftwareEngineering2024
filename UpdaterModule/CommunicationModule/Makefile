HOST = 127.0.0.1
PORT = 8080
SERVERFILEPATH = /Users/amithabh_a/Projects/FileTransfer/server
CLIENTFILEPATH = /Users/amithabh_a/Projects/FileTransfer/c1

all: server

server: build
	dotnet run server $(PORT) $(SERVERFILEPATH)

client: build
	dotnet run $(HOST) $(PORT) $(CLIENTFILEPATH)

build:
	dotnet build

clean:
	dotnet clean
