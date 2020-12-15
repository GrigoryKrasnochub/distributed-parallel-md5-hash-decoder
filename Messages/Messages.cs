using System;
using System.Collections.Generic;
using System.Text;

namespace ClientServerCSharp.Messages
{
    class Messages
    {
        private List<string> messages = new List<string>();

        public string ModuleName { get; protected set; }

        protected void AddMessage(string message)
        {
            lock (messages)
            {
                messages.Add(message);
            }
        }

        public virtual string[] GetMessages()
        {
            lock (messages)
            {
                string[] message = messages.ToArray();
                messages.Clear();
                return message;
            }
        }
    }
}
