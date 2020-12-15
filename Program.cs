using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using ClientServerCSharp.Client;

namespace ClientServerCSharp
{
    class Program
    {
        static Client.Client ServClient;
        static Server.Server Server;
        static Settings.Settings Settings;
        static List<Messages.Messages> ModulesMessages = new List<Messages.Messages>();

        static Regex CommandRegex = new Regex(@"\s+");

        static void Main(string[] args)
        {
            Settings = new Settings.Settings();
            ServClient = new Client.Client(ref Settings);
            Server = new Server.Server();

            ModulesMessages.Add(ServClient);
            ModulesMessages.Add(Server);

            Thread PrintModuleMessagesThread = new Thread(new ThreadStart(PrintModuleMessages));
            PrintModuleMessagesThread.Start();
            Greatings();
            DoCommands();
        }

        static void Greatings()
        {
            Console.WriteLine("MD5 hash decoder");
        }

        static void PrintModuleMessages()
        {
            while (true)
            {
                bool printed = false;
                foreach (Messages.Messages moduleMessages in ModulesMessages)
                {
                    foreach (string message in moduleMessages.GetMessages())
                    {
                        Console.WriteLine(moduleMessages.ModuleName + ": " + message);
                        printed = true;
                    }
                }
                if (printed)
                {
                    Console.Write(">");
                }
            }
        }

        static void DoCommands()
        {
            while (true)
            {
                Console.Write(">");
                string command = Console.ReadLine();
                if (command.Length > 0)
                {
                    string[] commands = CommandRegex.Split(command);

                    if (commands.Length > 0)
                    {
                        switch (commands[0])
                        {
                            case "i":
                                Greatings();
                                Console.WriteLine("Comands list:");
                                Console.WriteLine("____SHARED____");
                                Console.WriteLine("i - info;");
                                Console.WriteLine("ss - show/set settings; full format {c -ip hostIp -p hostPort -l lettersMax -c maxCalcClients}");
                                Console.WriteLine("____CLIENT____");
                                Console.WriteLine("c - connect as client; full format {c -ip hostIp -p hostPort}");
                                Console.WriteLine("d - disconect as client");
                                Console.WriteLine("____SERVER____");
                                Console.WriteLine("s - start server; full format  {s -p port}");
                                Console.WriteLine("sd - start decodind");
                                break;

                            case "d":
                                Console.WriteLine("Disconect");
                                ServClient.Disconnect();
                                ServClient.Disconnect();
                                break;

                            case "ss":
                                Settings.parseSettings(commands);
                                Console.WriteLine(Settings.ToString());
                                break;

                            case "s":
                                Settings.parseSettings(commands);
                                Server.StartServer(Settings);
                                break;

                            // TODO добавить проверку на запущенность сервера
                            case "sd":
                                if (commands.Length > 1)
                                {
                                    string[] result = Server.DoCalculations(HashDecoder.CreateMD5(commands[1]));
                                    foreach (string res in result)
                                    {
                                        Console.WriteLine(res);
                                    }
                                } else
                                {
                                    Console.WriteLine("not enought arguments");
                                }
                                break;

                            case "c":
                                Settings.parseSettings(commands);
                                Thread Client = new Thread(new ThreadStart(ServClient.Connect));
                                Client.Start();
                                break;

                            default:
                                Console.WriteLine("unknown command (i for list of commands)");
                                break;
                        }
                    }
                }
            }
        }
    }
}
