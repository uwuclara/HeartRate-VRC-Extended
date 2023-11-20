using System;
using System.Net;
using System.Net.Sockets;

namespace HRPresence
{
    class OSC : Program
    {
        protected UdpClient udp;
        
        public void Initialize(IPAddress ip, int port)
        {
            udp = new UdpClient();
            udp.Connect(ip, port);
            
            if (config.useChatOSC)
            {
                OSCChatBox.InitializeChat(udp);
                Console.WriteLine("> [Config] OSC ChatBox [on]");
            }
                
            if (config.useParametersOSC)
            {
                Console.WriteLine("> [Config] OSC Parameters [on]");
            }
        }
    }
}