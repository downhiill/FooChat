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
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;
        private readonly CustomTcpListener _tcpListener; // Ссылка на CustomTcpListener для вызова общих методов
        private string _name;

        /// <summary>
        /// Событие, возникающее при подключении клиента.
        /// </summary>
        public event Action<string> ClientConnected;

        /// <summary>
        /// Событие, возникающее при получении сообщения от клиента.
        /// </summary>
        public event Action<string> MessageReceived;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="Client"/> с указанным TCP-клиентом и слушателем.
        /// </summary>
        /// <param name="tcpClient">TCP-клиент, представляющий подключение клиента.</param>
        /// <param name="tcpListener">Слушатель TCP для получения и отправки сообщений.</param>
        public Client(TcpClient tcpClient, CustomTcpListener tcpListener)
        {
            _tcpClient = tcpClient;
            _tcpListener = tcpListener;
            _stream = tcpClient.GetStream();
        }

        /// <summary>
        /// Обрабатывает взаимодействие с клиентом, включая получение имени и рассылку сообщений.
        /// </summary>
        /// <returns>Асинхронная задача.</returns>
        public async Task HandleClientAsync()
        {
            try
            {
                _name = await ReceiveNameAsync();
                ClientConnected?.Invoke(_name); // Генерируем событие при подключении клиента

                while (true)
                {
                    string message = await _tcpListener.ReceiveMessageAsync(_stream);
                    MessageReceived?.Invoke($"{_name}: {message}"); // Генерируем событие при получении сообщения
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Клиент {_name} отключился: {ex.Message}");
            }
            finally
            {
                _tcpClient.Close();
            }
        }

        /// <summary>
        /// Получает имя клиента.
        /// </summary>
        /// <returns>Имя клиента.</returns>
        private async Task<string> ReceiveNameAsync()
        {
            return await _tcpListener.ReceiveMessageAsync(_stream);
        }

        /// <summary>
        /// Отправляет сообщение клиенту.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        /// <returns>Асинхронная задача.</returns>
        public async Task SendMessageAsync(string message)
        {
            await _tcpListener.SendMessageAsync(_stream, message);
        }
    }
}
