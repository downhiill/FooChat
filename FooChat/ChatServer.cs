using FooChatServer;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FooChat
{
    /// <summary>
    /// Представляет чат-сервер, который управляет подключениями клиентов и их взаимодействием.
    /// </summary>
    class ChatServer
    {
        private readonly CustomTcpListener _listener;
        private readonly ClientManager _clientManager;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ChatServer"/> с указанным портом.
        /// </summary>
        /// <param name="port">Порт, на котором сервер будет слушать входящие подключения.</param>
        public ChatServer(int port)
        {
            _listener = new CustomTcpListener(IPAddress.Any, port);
            _clientManager = new ClientManager(_listener); 
        }

        /// <summary>
        /// Запускает сервер и начинает ожидание подключений от клиентов.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        public async Task StartAsync()
        {
            _listener.Start();
            Console.WriteLine("Сервер запущен... Ожидание подключений");

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _clientManager.AddClient(client);
            }
        }
    }
}