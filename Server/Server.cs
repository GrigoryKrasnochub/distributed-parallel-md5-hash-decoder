using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace ClientServerCSharp.Server
{
    class Server : Messages.Messages
    {
        static ServerObject server; // сервер
        static Thread listenThread; // потока для прослушивания

        public Server()
        {
            ModuleName = "Server";
        }

        public void StartServer(Settings.Settings settings)
        {
            try
            {
                server = new ServerObject(ref settings);
                listenThread = new Thread(new ThreadStart(server.Listen));
                listenThread.Start();
            }
            catch (Exception ex)
            {
                server.Disconnect();
                Console.WriteLine(ex.Message);
            }
        }

        public string[] DoCalculations(string hash)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            string[] calcResult = server.DoCalculations(hash).ToArray();
            sw.Stop();
            Console.WriteLine("Calculation time: {0} milliseconds", sw.ElapsedMilliseconds.ToString()); 
            //(String.Format("Calculation time: {0} milliseconds",sw.ElapsedMilliseconds.ToString()));
            return calcResult;
        }

        public override string[] GetMessages()
        {
            if (server != null) {
                return server.GetMessages();
            } else {
                return new string[0];
            }
        }
    }
}
