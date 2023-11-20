
using System.Net.Sockets;

namespace HRPresence
{
    internal class OSCParameters
    {
        private readonly UdpClient _udpClient;
        private readonly string _parInt;
        private readonly string _parFloat;
        private readonly string _parOnes;
        private readonly string _parTens;
        private readonly string _parHundreds;
        
        public OSCParameters(UdpClient udp, string parInt, string parFloat, string parOnes, string parTens, string parHundreds)
        {
            _udpClient = udp;
            _parInt = parInt;
            _parFloat = parFloat;
            _parOnes = parOnes;
            _parTens = parTens;
            _parHundreds = parHundreds;
        }
        
        public void UpdateParameters(int heartbeat) 
        {
            var data = new (string, object)[] 
            {
                (_parInt       ,heartbeat),
                (_parFloat     ,heartbeat * 0.0078125f - 1.0f),
                (_parOnes      ,heartbeat       % 10),
                (_parTens      ,heartbeat / 10  % 10),
                (_parHundreds  ,heartbeat / 100 % 10)
            };

            try 
            {
                foreach (var (path, value) in data) 
                {
                    var bytes = new OscCore.OscMessage($"/avatar/parameters/{path}", value).ToByteArray();
                    _udpClient.Send(bytes, bytes.Length);
                }
            } 
            catch { }
        }
    }
}
