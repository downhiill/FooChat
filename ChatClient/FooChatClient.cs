using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    class FooChatClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

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

        private async Task ReceiveMessageAsync()
        {
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer);
                    if (bytesRead == 0) break; // Соединение закрыто сервером

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    Console.WriteLine(message);
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
