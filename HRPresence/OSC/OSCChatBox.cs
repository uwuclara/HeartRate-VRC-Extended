using System;
using System.Net.Sockets;

namespace HRPresence
{
	internal class OSCChatBox
    {
        private readonly UdpClient _udpClient;
        private readonly string[] _chatBoxFormat;
        
        public OSCChatBox(UdpClient udp, string chatBoxMessage = "HeartRate: %HR% BPM <3")
        {
            _udpClient = udp;
            
            var format = chatBoxMessage.Split(new [] { "%HR%" }, StringSplitOptions.None);
            
            if (format.Length == 2)
            {
                _chatBoxFormat = format;
            }
            else
            {
                Console.WriteLine("Bad format for chatOSCMessage in config! Falling to default.");
                _chatBoxFormat = new[] { "HeartRate: ", " BPM <3" };
            }
        }
        
        internal void updateChatBox(int heartbeat)
        {
            try
            {
                var bytes = new OscCore.OscMessage("/chatbox/input", _chatBoxFormat[0] + heartbeat + _chatBoxFormat[1], true, false).ToByteArray();
                _udpClient.Send(bytes, bytes.Length);
            }
            catch { } // sometimes its not available and this is meant to be always try/reconnect
        }
    }
}