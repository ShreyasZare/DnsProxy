using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DnsProxy
{
    static class SocketExtension
    {

        public static Task<int> SendToAsync(this Socket socket, byte[] buffer, int offset, int size, EndPoint remoteEP, SocketFlags socketFlags = SocketFlags.None)
        {
            return Task.Factory.FromAsync(
                delegate (AsyncCallback callback, object state)
                {
                    return socket.BeginSendTo(buffer, offset, size, socketFlags, remoteEP, callback, state);
                },
                delegate (IAsyncResult result)
                {
                    return socket.EndSendTo(result);
                },
                null);
        }

    }
}
