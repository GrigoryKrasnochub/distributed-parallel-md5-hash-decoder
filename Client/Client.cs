using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ClientServerCSharp.Client
{
    class Client : Messages.Messages
    {
        TcpClient client;
        NetworkStream stream;
        Settings.Settings Settings;

        public Client(ref Settings.Settings settings)
        {
            ModuleName = "client";
            Settings = settings;
        }

        private void SendMessage(string message)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }

        public void Connect()
        {
            client = new TcpClient();
            try
            {
                client.Connect(Settings.Host, Settings.Port); //подключение клиента
                stream = client.GetStream(); // получаем поток

                // запускаем новый поток для получения данных
                AddMessage("Client connected. Waiting for Server commands...");
               
                while (true){
                    ReceiveMessage();
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                Disconnect();
            }
        }

        private void ReceiveMessage()
        {
            try
            {
                byte[] data = new byte[64]; // буфер для получаемых данных
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                string message = builder.ToString();
                AddMessage("get command");
                AddMessage(message);

                if (message.Length > 0)
                {
                    string[] messageParts = message.Split(" ");
                    if (messageParts.Length > 0)
                    {
                        switch (messageParts[0])
                        {
                            case "decode":
                                if (messageParts.Length < 2)
                                {
                                    AddMessage("err, not enought arguments");
                                    break;
                                }
                               
                                List<int> letters = new List<int>();
                                foreach(string letter in messageParts[1].Split(";"))
                                {
                                    int val;
                                    if(int.TryParse(letter, out val))
                                    {
                                        letters.Add(val);
                                    }
                                }

                                if (letters.Count < 1)
                                {
                                    AddMessage("err, 0 letters count");
                                    break;
                                }

                                DoCalc(letters, messageParts[2]);

                                break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                AddMessage(String.Format("loose connection {0}", e.Message));
                Console.ReadLine();
                Disconnect();
            }
        }

        public void DoCalc(List<int> lettersCount, string hash)
        {
            AddMessage(String.Format("Start decoding lettersCount: {0}; hash:{1}", lettersCount, hash));
            List<string> result = HashDecoder.DecodeMD5Hash(lettersCount.ToArray(), hash, Settings.Dictionary);
            string resStr = String.Join(";", result.ToArray());
            AddMessage("Calculation finished! Result: " + resStr);
            SendMessage("200 " + resStr);
        }

        public void Disconnect()
        {
            if (stream != null)
                stream.Close();
            if (client != null)
                client.Close();
            Environment.Exit(0);
        }
    }
}
