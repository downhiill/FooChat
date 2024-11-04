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
        private readonly CustomTcpListener _tcpListener;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="ClientManager"/> с указанным слушателем TCP.
        /// </summary>
        /// <param name="tcpListener">Слушатель TCP, используемый для получения входящих подключений.</param>
        public ClientManager(CustomTcpListener tcpListener)
        {
            _tcpListener = tcpListener;
        }

        /// <summary>
        /// Добавляет нового клиента в менеджер клиентов и начинает обрабатывать его асинхронно.
        /// </summary>
        /// <param name="tcpClient">TCP-клиент, представляющий новое подключение.</param>
        public void AddClient(TcpClient tcpClient)
        {
            var client = new Client(tcpClient, this, _tcpListener); 
            _clients[tcpClient] = client;
            Task.Run(() => client.HandleClientAsync());
        }

        /// <summary>
        /// Удаляет клиента из менеджера клиентов и рассылает сообщение о его выходе.
        /// </summary>
        /// <param name="tcpClient">TCP-клиент, который нужно удалить.</param>
        /// <param name="clientName">Имя клиента, который покидает чат.</param>
        public void RemoveClient(TcpClient tcpClient, string clientName)
        {
            if (_clients.TryRemove(tcpClient, out _))
            {
                BroadcastMessage($"{clientName} покинул чат");
            }
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
                client.SendMessageAsync(message); 
            }
        }
    }
}
