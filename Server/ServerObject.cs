using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace ClientServerCSharp.Server
{
    class ServerObject : Messages.Messages
    {
        static TcpListener tcpListener; // сервер для прослушивания
        List<ClientObject> clients = new List<ClientObject>(); // все подключения
        Settings.Settings Settings;
        
        public ServerObject(ref Settings.Settings settings)
        {
            Settings = settings;
        }

        private void ResetClients()
        {
            foreach (ClientObject client in clients)
            {
                client.ResetResults();
            }
        }

        protected internal void AddConnection(ClientObject clientObject)
        {
            clients.Add(clientObject);
        }
        protected internal void RemoveConnection(string id)
        {
            // получаем по id закрытое подключение
            ClientObject client = clients.FirstOrDefault(c => c.Id == id);
            // и удаляем его из списка подключений
            if (client != null)
                clients.Remove(client);
        }
        // прослушивание входящих подключений
        protected internal void Listen()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, Settings.Port);
                tcpListener.Start();
                AddMessage("Start server. Port " + Settings.Port);

                while (true)
                {
                    TcpClient tcpClient = tcpListener.AcceptTcpClient();

                    ClientObject clientObject = new ClientObject(tcpClient, this);
                    Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                    clientThread.Start();
                }
            }
            catch (Exception ex)
            {
                AddMessage(ex.Message);
                Disconnect();
            }
        }
        
        public List<string> DoCalculations(string hash)
        {
            ResetClients();
            int maxClientsCount = Settings.Clients;
            int count = clients.Count;
            count = count > maxClientsCount ? maxClientsCount : count;
            count = count > Settings.Letters ? Settings.Letters : count;

            //разбиение на клиенты
            /*
            int[] combinations = new int[Settings.Letters];
            int dictionaryLen = Settings.Dictionary.Length;
            for (int i = 1; i <= Settings.Letters; i++) {
                combinations[i - 1] = Enumerable.Range(1, dictionaryLen + i - 1).Aggregate(1, (p, item) => p * item) / 
                    (Enumerable.Range(1,  i - 1).Aggregate(1, (p, item) => p * item) * (Enumerable.Range(1, dictionaryLen).Aggregate(1, (p, item) => p * item)));
            }
            */
            string[] clientsLetters = new string[count];
            for (int i = 1; i <= Settings.Letters;)
            {
                for (int j = 0; j < count; j++)
                {
                    clientsLetters[j] += i.ToString() + ";";
                    i++;
                    if (i > Settings.Letters)
                    {
                        break;
                    }
                }
            }

            int activeClients = count;
            foreach (ClientObject client in clients)
            {
                activeClients--;
                sendMessage(String.Format("decode {0} {1}", clientsLetters[activeClients], hash), client.Id);
                if (activeClients == 0)
                {
                    break;
                }
            }

            int clientsCount = 0;
            while(true)
            {
                //TODO check only clients, that calculate
                foreach (ClientObject client in clients)
                {
                    if (client.Status == true)
                    {
                        clientsCount++;
                    }
                }
                if (count == clientsCount)
                {
                    break;
                }
                clientsCount = 0;
            }

            List<string> CalcResults = new List<string>();

            foreach (ClientObject client in clients)
            {
                if (client.Status == true)
                {
                    clientsCount++;
                    CalcResults.AddRange(client.Result);
                }
            }


            return CalcResults;
        }

        // трансляция сообщения подключенным клиентам
        protected internal void sendMessage(string message, string id)
        {
            byte[] data = Encoding.Unicode.GetBytes(message);
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].Id == id)
                {
                    clients[i].Stream.Write(data, 0, data.Length); //передача данных
                    break;
                }
            }
        }

        // отключение всех клиентов
        protected internal void Disconnect()
        {
            tcpListener.Stop(); //остановка сервера

            for (int i = 0; i < clients.Count; i++)
            {
                clients[i].Close(); //отключение клиента
            }
            Environment.Exit(0); //завершение процесса
        }

        public override string[]  GetMessages()
        {
            string[] servMessages = base.GetMessages();
            List<string> Messages = new List<string>();

            foreach (string messages in servMessages)
            {
                Messages.Add(messages);
            }

            foreach (ClientObject client in clients)
            {
                if (client == null)
                {
                    continue;
                }
                foreach ( string messages in client.GetMessages())
                {
                    Messages.Add(messages);
                }
            }

            return Messages.ToArray();
        }
    }
}
