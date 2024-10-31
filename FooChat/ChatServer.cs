using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChat
{
    class ChatServer
    {
        private  TcpListener _listener;
        private ConcurrentDictionary<TcpClient, string> _clients = new();

        public async Task StartAsync()
        {
            _listener = new TcpListener(IPAddress.Any, 5000);
            _listener.Start();
            Console.WriteLine("Сервер запущен... Ожидание подключений.");

            while(true) 
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var clientEndPoint = client.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"Клиент {clientEndPoint} подключился.");

            using var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = await stream.ReadAsync(buffer);
            string name = Encoding.UTF8.GetString(buffer,0, bytesRead);

            _clients[client] = name;
            BroadcastMessage($"{name} присоединился к чату");

            try
            {
                while(true)
                {
                    bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    BroadcastMessage($"{name}: {message}");
                }
            }
            catch
            {
                Console.WriteLine($"Клиент {clientEndPoint} отключился.");
            }
            finally
            {
                _clients.TryRemove(client, out _);
                BroadcastMessage($"{name} покинул чат");
                client.Close();
            }
        }

        private void BroadcastMessage(string message)
        {
            Console.WriteLine(message);
            byte[] data =  Encoding.UTF8.GetBytes(message);

            Parallel.ForEach(_clients.Keys, client =>
            {
                try
                {
                    client.GetStream().WriteAsync(data);
                }
                catch
                {
                    // Игнорируем ошибки записи для отключенных клиентов
                }
            });
        }
    }
}
