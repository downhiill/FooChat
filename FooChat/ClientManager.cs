using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChatServer
{
    internal class ClientManager
    {
        private readonly ConcurrentDictionary<TcpClient, Client> _clients = new();

        public void AddClient(TcpClient tcpClient)
        {
            var client = new Client(tcpClient, this);
            _clients[tcpClient] = client;
            Task.Run(() => client.HandleClientAsync());
        }
        
        public void RemoveClient(TcpClient tcpClient, string clientName)
        {
            if(_clients.TryRemove(tcpClient, out _))
            {
                BroadcastMessage($"{clientName} покинул чат");
            }
        }

        public void BroadcastMessage(string message)
        {
            Console.WriteLine(message);
            foreach(var client in _clients.Values)
            {
                client.SendMessage(message);
            }
        }
    }
}
