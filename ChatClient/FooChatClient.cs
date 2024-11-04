using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    /// <summary>
    /// Класс для клиента чата, который обрабатывает подключение к серверу и обмен сообщениями.
    /// </summary>
    class FooChatClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        /// <summary>
        /// Асинхронно подключается к чату, отправляя имя пользователя на сервер.
        /// </summary>
        /// <param name="userName">Имя пользователя, которое будет отправлено на сервер.</param>
        public async Task ConnectAsync(string userName)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", 5000);
                _stream = _client.GetStream();

                // Отправляем длину имени (4 байта для int) и затем само имя
                byte[] nameLengthData = BitConverter.GetBytes(Encoding.UTF8.GetByteCount(userName));
                await _stream.WriteAsync(nameLengthData, 0, nameLengthData.Length);

                byte[] nameData = Encoding.UTF8.GetBytes(userName);
                await _stream.WriteAsync(nameData, 0, nameData.Length);

                Console.WriteLine("Подключение успешно! Можете отправлять сообщения");

                // Запуск задачи для получения сообщений
                _ = Task.Run(ReceiveMessageAsync);

                // Цикл для отправки сообщений
                while (true)
                {
                    string message = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(message)) continue;

                    await SendMessageAsync(message);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка соединения: {ex.Message}");
            }
            finally
            {
                _stream?.Close();
                _client?.Close();
            }
        }

        /// <summary>
        /// Асинхронно отправляет сообщение на сервер.
        /// </summary>
        /// <param name="message">Сообщение для отправки.</param>
        private async Task SendMessageAsync(string message)
        {
            try
            {
                // Отправляем длину сообщения (4 байта для int) и затем само сообщение
                byte[] messageLengthData = BitConverter.GetBytes(Encoding.UTF8.GetByteCount(message));
                await _stream.WriteAsync(messageLengthData, 0, messageLengthData.Length);

                byte[] messageData = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(messageData, 0, messageData.Length);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Ошибка при отправке данных: {ex.Message}");
            }
        }

        /// <summary>
        /// Асинхронно получает сообщения от сервера.
        /// </summary>
        private async Task ReceiveMessageAsync()
        {
            while (true)
            {
                try
                {
                    // Читаем первые 4 байта для определения длины сообщения
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = await _stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break; // Сервер закрыл соединение

                    int messageLength = BitConverter.ToInt32(lengthBuffer, 0);

                    // Создаем буфер для хранения полного сообщения
                    byte[] messageBuffer = new byte[messageLength];
                    int totalBytesRead = 0;

                    // Читаем сообщение в цикле, чтобы получить полное сообщение
                    while (totalBytesRead < messageLength)
                    {
                        bytesRead = await _stream.ReadAsync(messageBuffer, totalBytesRead, messageLength - totalBytesRead);
                        if (bytesRead == 0) break; // Сервер закрыл соединение
                        totalBytesRead += bytesRead;
                    }

                    // Проверяем, что все байты сообщения получены
                    if (totalBytesRead == messageLength)
                    {
                        string message = Encoding.UTF8.GetString(messageBuffer);
                        Console.WriteLine(message);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Ошибка при получении данных: {ex.Message}");
                    break;
                }
            }
        }
    }
}
