using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChatServer
{
    /// <summary>
    /// Представляет обработчик клиента, управляющий обменом данными с клиентом.
    /// </summary>
    public class ClientHandler : IDisposable
    {
        private readonly TcpClient _tcpClient;
        private readonly NetworkStream _stream;

        public event Action<string, string> MessageReceived;

        public ClientHandler(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _stream = _tcpClient.GetStream();
        }

        public async Task<string> ReceiveClientNameAsync()
        {
            return await ReceiveMessageAsync();
        }

        public async Task StartListeningForMessagesAsync(string clientName)
        {
            while (_tcpClient.Connected)
            {
                string message = await ReceiveMessageAsync();
                MessageReceived?.Invoke(clientName, message);
            }
        }

        private async Task<string> ReceiveMessageAsync()
        {
            int messageLength = await ReadIntAsync();
            byte[] messageBuffer = new byte[messageLength];
            await _stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
            return Encoding.UTF8.GetString(messageBuffer);
        }

        public async Task SendMessageAsync(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4);
            await _stream.WriteAsync(data, 0, data.Length);
        }

        private async Task<int> ReadIntAsync()
        {
            byte[] buffer = new byte[4];
            await _stream.ReadAsync(buffer, 0, buffer.Length);
            return BitConverter.ToInt32(buffer, 0);
        }

        public void Dispose()
        {
            _stream?.Dispose();
            _tcpClient?.Close();
        }
    }
}
