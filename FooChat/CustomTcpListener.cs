using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FooChatServer
{
    class CustomTcpListener
    {
        private readonly TcpListener _listener;

        public CustomTcpListener(IPAddress address, int port)
        {
            _listener = new TcpListener(address, port);
        }

        public void Start()
        {
            _listener.Start();
        }

        public async Task<TcpClient> AcceptTcpClientAsync()
        {
            return await _listener.AcceptTcpClientAsync();
        }

        public void Stop()
        {
            _listener.Stop();
        }
    }
}
