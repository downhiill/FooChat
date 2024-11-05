using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace ChatClient
{
    public class FooChatClient
    {
        private TcpClient _client;
        private NetworkStream _stream;

        public event Action<string> MessageReceived;
        public event Action Connected;
        public event Action Disconnected; // Событие отключения от сервера

        public async Task ConnectAsync(string userName)
        {
            try
            {
                _client = new TcpClient();
                await _client.ConnectAsync("127.0.0.1", 5000);
                _stream = _client.GetStream();

                await SendMessageAsync(userName);
                Connected?.Invoke();

                _ = Task.Run(ReceiveMessagesAsync);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка подключения: {ex.Message}");
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                while (true)
                {
                    int messageLength = await ReadMessageLengthAsync();
                    byte[] buffer = new byte[messageLength];
                    await _stream.ReadAsync(buffer, 0, messageLength);

                    string message = Encoding.UTF8.GetString(buffer);
                    MessageReceived?.Invoke(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при приеме сообщения: {ex.Message}");
                Disconnected?.Invoke(); // Вызываем событие разрыва связи
            }
            finally
            {
                _client?.Close(); // Закрываем клиентское подключение при ошибке
            }
        }

        public async Task Disconnect()
        {
            if (_client != null && _client.Connected)
            {
                _client.Close();
                Disconnected?.Invoke();
            }
        }

        private async Task<int> ReadMessageLengthAsync()
        {
            byte[] lengthBuffer = new byte[4];
            await _stream.ReadAsync(lengthBuffer, 0, lengthBuffer.Length);
            return BitConverter.ToInt32(lengthBuffer, 0);
        }

        public async Task SendMessageAsync(string message)
        {
            if (_stream == null) throw new InvalidOperationException("Нет подключения к серверу");

            byte[] messageData = Encoding.UTF8.GetBytes(message);
            byte[] messageLength = BitConverter.GetBytes(messageData.Length);

            await _stream.WriteAsync(messageLength, 0, messageLength.Length);
            await _stream.WriteAsync(messageData, 0, messageData.Length);
        }

       
    }
}
