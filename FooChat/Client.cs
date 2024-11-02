using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChatServer
{
     class Client
    {
        private readonly TcpClient _tcpClient;
        private readonly ClientManager _manager;
        private readonly NetworkStream _stream;
        private string _name;

        public Client(TcpClient tcpClient, ClientManager manager)
        {
            _tcpClient = tcpClient;
            _manager = manager;
            _stream = tcpClient.GetStream();
        }

        public async Task HandleClientAsync()
        {
            try
            {
                _name = await ReceiveNameAsync();
                _manager.BroadcastMessage($"{_name} присоединился к чату");

                while(true)
                {
                    string message = await ReceiveMessageAsync();
                    _manager.BroadcastMessage($"{_name}: {message}");
                }
            }
            catch
            {
                Console.WriteLine($"Клиент {_name} отключился");
            }
            finally
            {
                _manager.RemoveClient(_tcpClient, _name);
                _tcpClient.Close();
            }
        }

        private async Task<string> ReceiveNameAsync()
        {
            int nameLength = await ReadIntAsync();
            byte[] nameBuffer = new byte[nameLength];
            await _stream.ReadAsync( nameBuffer, 0, nameBuffer.Length );
            return Encoding.UTF8.GetString( nameBuffer );
        }

        private async Task<string> ReceiveMessageAsync()
        {
            int messageLength = await ReadIntAsync();
            byte[] messageBuffer = new byte[messageLength];
            await _stream.ReadAsync(messageBuffer, 0, messageBuffer.Length );
            return Encoding.UTF8.GetString( messageBuffer );
        }

        private async Task<int> ReadIntAsync()
        {
            byte[] buffer = new byte[4];
            await _stream.ReadAsync( buffer, 0, buffer.Length );
            return BitConverter.ToInt32( buffer, 0 );
        }

        public void SendMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            _stream.WriteAsync(data,0, data.Length );
        }
    }
}
