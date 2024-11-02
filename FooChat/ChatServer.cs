using FooChatServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChat
{
    class ChatServer
    {
        private readonly CustomTcpListener _listener;
        private readonly ClientManager _clientManager;

        public ChatServer(int port)
        {
            _listener = new CustomTcpListener(IPAddress.Any, port);
            _clientManager = new ClientManager();
        }

       public async Task StartAsync()
       {
            _listener.Start();
            Console.WriteLine("Сервер запущен... Ожидание подключений");

            while(true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clientManager.AddClient(client);
            }
       }
    }
}
