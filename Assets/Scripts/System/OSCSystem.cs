using System.Net;
using OscJack;
using UnityEngine;

namespace System
{
    public static class OSCSystem
    {
        private static OscClient _client;
        private static IPAddress _ip = IPAddress.Parse("255.255.255.255");
        private static int _port = 9000;
        
        public static bool HasFunctionalClient => _client != null;

        static OSCSystem()
        {
            RecreateClient();
        }
        
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
            try
            {
                _client = new OscClient(_ip.ToString(), _port);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error creating OSC client: {e}");
                _client = null;
            }
        }

        public static void Send(string address, float valueToSend)
        {
            if (_client == null)
                return;

            try
            {
                _client.Send(address, valueToSend);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending OSC message: {e}");
            }
        }

        public static void Send(string address, int valueToSend)
        {
            if (_client == null)
                return;

            try
            {
                _client.Send(address, valueToSend);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error sending OSC message: {e}");
            }
        }
    }
}