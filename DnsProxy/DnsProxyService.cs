using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace DnsProxy
{
    class DnsProxyService
    {
        IPEndPoint _dnsServer = new IPEndPoint(IPAddress.Parse("8.8.8.8"), 53);
        readonly Socket _udpListener;

        public DnsProxyService()
        {
            _udpListener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _udpListener.Bind(new IPEndPoint(IPAddress.Any, 53));

            int listenerThreadCount = Math.Max(1, Environment.ProcessorCount);

            for (int i = 0; i < listenerThreadCount; i++)
                ThreadPool.QueueUserWorkItem(ReadUdpRequestAsync);
        }

        private void ReadUdpRequestAsync(object state)
        {
            EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] recvBuffer = new byte[512];
            int bytesRecv;

            while (true)
            {
                bytesRecv = _udpListener.ReceiveFrom(recvBuffer, ref remoteEP);
                Console.WriteLine("ReceiveFrom: " + remoteEP.ToString());

                if (bytesRecv > 0)
                {
                    byte[] request = new byte[bytesRecv];
                    Buffer.BlockCopy(recvBuffer, 0, request, 0, request.Length);

                    _ = ProcessRequestAsync(request, remoteEP as IPEndPoint);
                }
            }
        }

        private async Task ProcessRequestAsync(byte[] request, IPEndPoint remoteEP)
        {
            try
            {
                UdpQuery(request, request.Length, out byte[] response, out int responseSize);

                Console.WriteLine("SendTo: " + remoteEP.ToString());

                //SendTo() blocking method works well
                //_udpListener.SendTo(response, 0, responseSize, SocketFlags.None, remoteEP); //---> works

                //SendToAsync() extension method fails
                await _udpListener.SendToAsync(response, 0, responseSize, remoteEP); //---> fails

                //BeginSendTo() too fails
                //IAsyncResult asyncResult = _udpListener.BeginSendTo(response, 0, responseSize, SocketFlags.None, remoteEP, null, null); //---> fails
                //_udpListener.EndSendTo(asyncResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void UdpQuery(byte[] request, int requestSize, out byte[] response, out int responseSize)
        {
            using (Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                client.Bind(new IPEndPoint(IPAddress.Any, 0));
                client.SendTo(request, 0, requestSize, SocketFlags.None, _dnsServer);

                EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
                response = new byte[1024];
                responseSize = client.ReceiveFrom(response, ref remoteEP);
            }
        }
    }
}
