using System;
using System.Threading;

namespace DnsProxy
{
    class Program
    {
        static void Main(string[] args)
        {
            DnsProxyService dnsProxy = new DnsProxyService();

            Console.WriteLine("DNS proxy is running. Press CTRL+C to terminate.");
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
