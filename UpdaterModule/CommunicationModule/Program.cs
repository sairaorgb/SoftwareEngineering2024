// Program.cs
using System;
using Communicator;

namespace CommunicationModule
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Please specify 'client' or 'server' as the first argument.");
                ShowUsage();
                return;
            }

            string mode = args[0].ToLower();

            switch (mode)
            {
                case "server":
                    if (args.Length != 3)
                    {
                        Console.WriteLine("Invalid number of arguments for server mode.");
                        ShowUsage();
                        return;
                    }

                    string serverPort = args[1];
                    string serverFolderPath = args[2];

                    Server server = new Server(serverFolderPath);
                    string serverStartResult = server.Start(null, serverPort);

                    if (serverStartResult != "failure")
                    {
                        Console.WriteLine($"Server is running at {serverStartResult}");
                        Console.WriteLine("Press ENTER to stop the server.");
                        Console.ReadLine();
                        server.Stop();
                    }
                    else
                    {
                        Console.WriteLine("Failed to start the server.");
                    }
                    break;

                case "client":
                    if (args.Length != 4)
                    {
                        Console.WriteLine("Invalid number of arguments for client mode.");
                        ShowUsage();
                        return;
                    }

                    string serverIP = args[1];
                    string clientPort = args[2];
                    string clientFolderPath = args[3];

                    Client client = new Client();
                    string clientStartResult = client.Start(serverIP, clientPort);

                    if (clientStartResult == "success")
                    {
                        Console.WriteLine("Connected to server successfully.");
                        client.SendFiles(clientFolderPath);
                        Console.WriteLine("All files have been sent.");
                        client.Stop();
                    }
                    else
                    {
                        Console.WriteLine("Failed to connect to the server.");
                    }
                    break;

                default:
                    Console.WriteLine("Invalid mode specified.");
                    ShowUsage();
                    break;
            }
        }

        static void ShowUsage()
        {
            Console.WriteLine("\nUsage:");
            Console.WriteLine("  As Server:");
            Console.WriteLine("    dotnet run server <port> <storageFolderPath>");
            Console.WriteLine("  As Client:");
            Console.WriteLine("    dotnet run client <serverIP> <port> <filesFolderPath>");
            Console.WriteLine("\nExamples:");
            Console.WriteLine("  dotnet run server 5000 \"C:\\FileStorage\"");
            Console.WriteLine("  dotnet run client 127.0.0.1 5000 \"C:\\ClientFiles\"");
        }
    }
}
