using Tomlyn.Model;

namespace HRPresence
{
    class Config : ITomlMetadataProvider
    {
        //Services
        public bool useBluetooth { get; set; } = false;
        public bool useHypeRate { get; set; } = false;
        public bool useStromno { get; set; } = false;
        
        //Services - Config
        public string HypeRate_or_Stromno_SessionID { get; set; } = "";
        public string HypeRateAPIkey { get; set; } = "";
        
        //Timeout
        public float TimeOutInterval { get; set; } = 4f;
        public float RestartDelay { get; set; } = 4f;
        public int serviceRefreshDelay  { get; set; } = 3;
        
        //Outputs
        public bool UseDiscordRPC { get; set; } = false;
        public bool useParametersOSC { get; set; } = false;
        public bool useChatOSC { get; set; } = false;
        public bool putToFile { get; set; } = false;
        
        //Outputs - Config
        public string DiscordRPCId { get; set; } = "1055058124266016808";
        public string discordGameDesc { get; set; } = "My HeartRate <3 is";
        public string OSCAddress  { get; set; } = "127.0.0.1";
        public int OSCPort { get; set; } = 9000;
        public string chatOSCMessage { get; set; } = "HeartRate: %HR% BPM <3";
        public string OSCHeartRateNameInt { get; set; } = "HR";
        public string OSCHeartRateNameFloat { get; set; } = "floatHR";
        public string OSCHeartRateNameOnes { get; set; } = "onesHR";
        public string OSCHeartRateNameTens { get; set; } = "tensHR";
        public string OSCHeartRateNameHundreds { get; set; } = "hundredsHR";

        //MetaData
        public TomlPropertiesMetadata PropertiesMetadata { get; set; }
    }
}