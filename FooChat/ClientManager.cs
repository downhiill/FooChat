using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FooChatServer
{
    /// <summary>
    /// Менеджер клиентов, который управляет подключенными клиентами и их сообщениями.
    /// </summary>
    internal class ClientManager
    {
        private readonly ConcurrentDictionary<TcpClient, Client> _clients = new();


        /// <summary>
        /// Добавляет нового клиента в менеджер клиентов и начинает обрабатывать его асинхронно.
        /// </summary>
        /// <param name="tcpClient">TCP-клиент, представляющий новое подключение.</param>
        public void AddClient(TcpClient tcpClient)
        {
            var client = new Client(tcpClient);
            client.ClientConnected += OnClientConnected;
            client.MessageReceived += OnMessageReceived;
            _clients[tcpClient] = client;

            // Начинаем обработку клиента асинхронно
            Task.Run(() => client.HandleClientAsync());
        }

        /// <summary>
        /// Обрабатывает событие подключения клиента.
        /// </summary>
        /// <param name="clientName">Имя подключившегося клиента.</param>
        private void OnClientConnected(string clientName)
        {
            BroadcastMessage($"{clientName} присоединился к чату");
        }

        /// <summary>
        /// Обрабатывает событие получения сообщения от клиента.
        /// </summary>
        /// <param name="message">Сообщение, полученное от клиента.</param>
        private void OnMessageReceived(string message)
        {
            BroadcastMessage(message);
        }

        /// <summary>
        /// Рассылает сообщение всем подключенным клиентам.
        /// </summary>
        /// <param name="message">Сообщение для рассылки.</param>
        public void BroadcastMessage(string message)
        {
            Console.WriteLine(message);
            foreach (var client in _clients.Values)
            {
                _ = client.SendMessageAsync(message); // Асинхронная отправка сообщения клиенту
            }
        }
    }
}
