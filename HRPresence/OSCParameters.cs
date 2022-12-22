
namespace HRPresence
{
    class OSCParameters : OSC
    {
        public void Update(int heartrate) 
        {
            var data = new (string, object)[] 
            {
                (config.OSCHeartRateNameInt        , heartrate),
                (config.OSCHeartRateNameFloat      , (heartrate * 0.0078125f) - 1.0f),
                (config.OSCHeartRateNameOnes       , (heartrate      ) % 10),
                (config.OSCHeartRateNameTens       , (heartrate / 10 ) % 10),
                (config.OSCHeartRateNameHundreds   , (heartrate / 100) % 10)
            };

            try 
            {
                foreach (var (path, value) in data) 
                {
                    var bytes = new OscCore.OscMessage($"/avatar/parameters/{path}", value).ToByteArray();
                    udp.Send(bytes, bytes.Length);
                }
            } 
            catch { }
        }
    }
}
