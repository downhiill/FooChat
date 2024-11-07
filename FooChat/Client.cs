using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FooChatServer
{
    /// <summary>
    /// Представляет клиента, подключенного к чату, и обрабатывает его сообщения.
    /// </summary>
    class Client
    {
        private readonly ClientHandler _tcpListener;
        private string _name;

        public event Action<string> ClientConnected;
        public event Action<string> MessageReceived;

        public Client(TcpClient tcpClient)
        {
            // Создаем экземпляр CustomTcpListener внутри Client
            _tcpListener = new ClientHandler(tcpClient);

            // Подписываемся на событие MessageReceived для обработки входящих сообщений
            _tcpListener.MessageReceived += OnMessageReceived;
        }

        /// <summary>
        /// Обрабатывает взаимодействие с клиентом, включая получение имени и подключение.
        /// </summary>
        public async Task HandleClientAsync()
        {
            try
            {
                _name = await _tcpListener.ReceiveClientNameAsync();
                ClientConnected?.Invoke(_name);

                // Начинаем прослушивание сообщений от клиента
                await _tcpListener.StartListeningForMessagesAsync(_name);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Клиент {_name} отключился: {ex.Message}");
            }
            finally
            {
                _tcpListener.MessageReceived -= OnMessageReceived;
                _tcpListener.Dispose();
            }
        }

        /// <summary>
        /// Отправляет сообщение клиенту.
        /// </summary>
        public async Task SendMessageAsync(string message)
        {
            await _tcpListener.SendMessageAsync(message);
        }

        /// <summary>
        /// Обработчик события получения сообщения.
        /// </summary>
        private void OnMessageReceived(string clientName, string message)
        {
            if (clientName == _name)
            {
                MessageReceived?.Invoke($"{_name}: {message}");
            }
        }
    }
}
