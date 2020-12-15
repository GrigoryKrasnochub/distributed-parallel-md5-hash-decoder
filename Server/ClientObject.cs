using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace ClientServerCSharp.Server
{
    class ClientObject : Messages.Messages
    {
        protected internal string Id { get; private set; }
        protected internal NetworkStream Stream { get; private set; }
        TcpClient client;
        ServerObject server; // объект сервера

        protected internal List<string> Result { get; private set; }
        protected internal bool Status { get; private set; }

        public ClientObject(TcpClient tcpClient, ServerObject serverObject)
        {
            Result = new List<string>();
            Status = false;
            Id = Guid.NewGuid().ToString();
            client = tcpClient;
            server = serverObject;
            serverObject.AddConnection(this);
        }

        public void ResetResults()
        {
            Result = new List<string>();
            Status = false;
        }

        public void Process()
        {
            try
            {
                Stream = client.GetStream();
                
                string message = this.Id + " conected";
                AddMessage(message);
                // в бесконечном цикле получаем сообщения от клиента
                while (true)
                {
                    try
                    {
                        message = GetMessage();
                        if (message == "")
                        {
                            continue;
                        }
                        message = String.Format("{0}: {1}", this.Id, message);
                        AddMessage(message);
                    }
                    catch
                    {
                        message = String.Format("{0}: leave", this.Id);
                        AddMessage(message);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                AddMessage(e.Message);
            }
            finally
            {
                // в случае выхода из цикла закрываем ресурсы
                server.RemoveConnection(this.Id);
                Close();
            }
        }

        // чтение входящего сообщения и преобразование в строку
        private string GetMessage()
        {
            byte[] data = new byte[64]; // буфер для получаемых данных
            StringBuilder builder = new StringBuilder();
            int bytes = 0;
            do
            {
                bytes = Stream.Read(data, 0, data.Length);
                builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
            }
            while (Stream.DataAvailable);

            string message = builder.ToString();
            string[] messageParts = message.Split(" ");
            int res;
            if (messageParts.Length > 1 && int.TryParse(messageParts[0], out res))
            {
                if (res == 200)
                {
                    Status = true;
                    string[] result = messageParts[1].Split(";");
                    foreach(string r in result)
                    {
                        Result.Add(r);
                    }
                }
            } else
            {
                AddMessage("Result parse ERR");
            }
            return message;
        }

        // закрытие подключения
        protected internal void Close()
        {
            if (Stream != null)
                Stream.Close();
            if (client != null)
                client.Close();
        }
    }
}
