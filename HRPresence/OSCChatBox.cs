using System;
using System.Net.Sockets;

namespace HRPresence
{
	class OSCChatBox : OSC
    {
        private static UdpClient udpS;
        private static string[] chatBoxFormat;
        
        public static void InitializeChat(UdpClient udp)
        {
            udpS = udp;
            var format = config.chatOSCMessage.Split(new [] { "%HR%" }, StringSplitOptions.None);
            
            if (format.Length == 2)
            {
                chatBoxFormat = format;
            }
            else
            {
                Console.WriteLine("Bad format for chatOSCMessage in config! Falling to default.");
                chatBoxFormat = new[] { "HeartRate: ", " BPM <3" };
            }
        }
        public static void Updated(int heartrate)
        {
            try
            {
                if (config.useChatOSC)
                {
                    var bytes = new OscCore.OscMessage("/chatbox/input", chatBoxFormat[0] + heartrate + chatBoxFormat[1], true, false).ToByteArray();
                    udpS.Send(bytes, bytes.Length);

                }
            }
            catch { }
        }
    }
}