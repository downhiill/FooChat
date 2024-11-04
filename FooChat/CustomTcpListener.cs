using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChatServer
{
    /// <summary>
    /// Представляет специальный TcpListener для работы с асинхронными операциями подключения и передачи данных.
    /// </summary>
    class CustomTcpListener
    {
        private readonly TcpListener _listener;

        /// <summary>
        /// Инициализирует новый экземпляр <see cref="CustomTcpListener"/> с заданным адресом и портом.
        /// </summary>
        /// <param name="address">Адрес, на котором сервер будет слушать.</param>
        /// <param name="port">Порт, на котором сервер будет слушать.</param>
        public CustomTcpListener(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
        }

        /// <summary>
        /// Запускает слушатель для ожидания входящих подключений.
        /// </summary>
        public void Start()
        {
            _listener.Start();
        }

        /// <summary>
        /// Асинхронно принимает входящее TCP-соединение.
        /// </summary>
        /// <returns>Объект <see cref="TcpClient"/>, представляющий подключившегося клиента.</returns>
        public async Task<TcpClient> AcceptTcpClientAsync()
        {
            return await _listener.AcceptTcpClientAsync();
        }

        /// <summary>
        /// Асинхронно получает строковое сообщение от клиента через заданный поток.
        /// </summary>
        /// <param name="stream">Поток, из которого будет считываться сообщение.</param>
        /// <returns>Содержимое полученного сообщения.</returns>
        public async Task<string> ReceiveMessageAsync(NetworkStream stream)
        {
            int messageLength = await ReadIntAsync(stream);
            byte[] messageBuffer = new byte[messageLength];
            await stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
            return Encoding.UTF8.GetString(messageBuffer);
        }

        /// <summary>
        /// Асинхронно отправляет строковое сообщение клиенту через заданный поток.
        /// </summary>
        /// <param name="stream">Поток, в который будет отправлено сообщение.</param>
        /// <param name="message">Сообщение для отправки.</param>
        public async Task SendMessageAsync(NetworkStream stream, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4); // Отправка длины сообщения
            await stream.WriteAsync(data, 0, data.Length); // Отправка данных сообщения
        }

        /// <summary>
        /// Асинхронно читает целое число из потока (например, длину сообщения).
        /// </summary>
        /// <param name="stream">Поток, из которого будет считано целое число.</param>
        /// <returns>Прочитанное целое число.</returns>
        private async Task<int> ReadIntAsync(NetworkStream stream)
        {
            byte[] buffer = new byte[4];
            await stream.ReadAsync(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }
    }
}
