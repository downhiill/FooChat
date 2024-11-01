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
            byte[] buffer = new byte[4]; // Буфер для чтения длины сообщения (4 байта для int)

            // Сначала читаем длину имени
            await stream.ReadAsync(buffer, 0, buffer.Length);
            int nameLength = BitConverter.ToInt32(buffer, 0);

            // Теперь читаем само имя
            byte[] nameBuffer = new byte[nameLength];
            await stream.ReadAsync(nameBuffer, 0, nameBuffer.Length);
            string name = Encoding.UTF8.GetString(nameBuffer);

            _clients[client] = name;
            BroadcastMessage($"{name} присоединился к чату");

            try
            {
                while (true)
                {
                    // Читаем длину сообщения
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break; // Клиент отключился

                    int messageLength = BitConverter.ToInt32(buffer, 0);

                    // Читаем сообщение
                    byte[] messageBuffer = new byte[messageLength];
                    bytesRead = await stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                    if (bytesRead == 0) break;

                    string message = Encoding.UTF8.GetString(messageBuffer);
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
