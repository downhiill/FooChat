using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace FooChatServer
{
    /// <summary>
    /// Серверный TcpListener для приёма новых подключений.
    /// </summary>
    public class ServerTcpListener
    {
        private readonly TcpListener _tcpListener;

        public ServerTcpListener(IPAddress ipAddress, int port)
        {
            _tcpListener = new TcpListener(ipAddress, port);
        }

        public void Start() => _tcpListener.Start();

        public async Task<TcpClient> AcceptTcpClientAsync()
        {
            return await _tcpListener.AcceptTcpClientAsync();
        }

        public void Stop() => _tcpListener.Stop();
    }
}
