using System;
using System.Collections.Generic;
using System.Linq;
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
            _client = new TcpClient();
            await _client.ConnectAsync("127.0.0.1", 5000);
            _stream = _client.GetStream();

            byte[] nameData = Encoding.UTF8.GetBytes(userName);
            await _stream.WriteAsync(nameData);

            Console.WriteLine("Подключение успешно! Можете отправлять сообщения");

            _ = Task.Run(ReceiveMessageAsync);
            
            while(true)
            {
                string message = Console.ReadLine();
                byte[] messageData = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(messageData);
            }
        }

        private async Task ReceiveMessageAsync()
        {
            byte[] buffer = new byte[1024];

            while(true)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0 , bytesRead);
                    Console.WriteLine(message);
                }
                catch
                {
                    Console.WriteLine("Ошибка подключения к серверу.");
                    break;
                }
            }
        }
    }
}
