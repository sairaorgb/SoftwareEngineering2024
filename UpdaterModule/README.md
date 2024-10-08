# Communicator
Sends files from client to server

# Steps to run
1. Clone the repository

2. To run the server
   ```bash
   make server <port-number> <filepath-where-server-stores-files>
   
   Console:
   dotnet run server <port> <filepath-where-server-stores-files>
   
   Example:
   dotnet run server 5000 "C:\received"
   
   ``` 
3. To run the client
   ```bash
   make client <ip-address> <port> <filepath-where-client-stores-files>

   Console:
   dotnet run client <ip-address> <port> <filepath-where-client-stores-files>
   
   Example:
   dotnet run client 192.168.56.1 5000 "C:\temp"
   ```
6. To clean
   ```bash
   make clean
   ```
