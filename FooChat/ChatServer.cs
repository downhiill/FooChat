using FooChatServer;
using System;
using System.Net;
using System.Threading.Tasks;

namespace FooChat
{
    class ChatServer
    {
        private readonly ServerTcpListener _listener;
        private readonly ClientManager _clientManager;

        public ChatServer(int port)
        {
            _listener = new ServerTcpListener(IPAddress.Any, port);
            _clientManager = new ClientManager(); // ClientManager не нужен слушатель напрямую
        }

        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Сервер запущен... Ожидание подключений");

            while (true)
            {
                var tcpClient = await _listener.AcceptTcpClientAsync();
                var clientHandler = new ClientHandler(tcpClient);

                // Вместо передачи clientHandler, передаем tcpClient
                _clientManager.AddClient(tcpClient);
            }
        }
    }
}
