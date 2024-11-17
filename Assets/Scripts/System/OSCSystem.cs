using System.Net;
using OscJack;

namespace System
{
    public static class OSCSystem
    {
        private static OscClient _client;
        private static IPAddress _ip = IPAddress.Parse("127.0.0.1");
        private static int _port = 9000;
        
        public static void SetPort(int port)
        {
            _port = port;
            RecreateClient();
        }

        public static void SetIp(IPAddress address)
        {
            _ip = address;
            RecreateClient();
        }

        private static void RecreateClient()
        {
            _client?.Dispose();
            _client = new OscClient(_ip.ToString(), _port);
        }

        public static void Send(string address, float valueToSend)
        {
            _client.Send(address, valueToSend);
        }
    }
}