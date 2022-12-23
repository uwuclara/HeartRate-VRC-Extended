using DiscordRPC;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using Tomlyn;

namespace HRPresence
{
    class Program
    {
        static DiscordRpcClient          discord;
        static HeartRateService          heartrate;
        static HeartRateReading          reading;
        static HypeRate_Stromno_Service  _hypeRateStromnoHeartRateStromno;
        static int                       hypeRate_Stromno_Reading;
        static OSCParameters             osc;
        static DateTime                  lastUpdate = DateTime.MinValue;
        protected static Config          config;
        
        static void Main() 
        {
            //Window
            Console.CursorVisible = false;
            Console.WindowHeight = 5;
            Console.WindowWidth = 32;
            
            //Config
            config = new Config();
            if (File.Exists("config.toml")) 
            {
                config = Toml.ToModel<Config>(File.OpenText("config.toml").ReadToEnd());
            } 
            else 
            {
                File.WriteAllText("config.toml", Toml.FromModel(config));
            }
            
            //Initialize Outputs
            if (config.UseDiscordRPC)
            {
                discord = new DiscordRpcClient(config.DiscordRPCId);
                discord.Initialize();
                Console.WriteLine("> [Config] Discord RPC [on]");
            }

            if (config.useParametersOSC || config.useChatOSC) 
            {
                osc = new OSCParameters();
                osc.Initialize(IPAddress.Parse(config.OSCAddress), config.OSCPort);
            }

            if (config.putToFile)
            {
                Console.WriteLine("> [Config] PutToFile [on]");
            }
            
            string service = null;
            
            //Services
            if(config.useBluetooth && !config.useHypeRate && !config.useStromno)
            {
                service = "Bluetooth";
                Console.WriteLine("> [Service] " + service);
                heartrate = new HeartRateService();
                heartrate.HeartRateUpdated += heart => 
                {
                    reading = heart;
                    
                    Console.Write($"{DateTime.Now}        \nService: (" + service + ")                       \n                               \n[HeartRate] " + reading.BeatsPerMinute + " BPM               \n                                 ");
                    Console.SetCursorPosition(0, 0);

                    lastUpdate = DateTime.Now;

                    if (config.putToFile)
                    {
                        File.WriteAllText("rate.txt", $"{reading.BeatsPerMinute}");
                    }

                    osc?.Update(reading.BeatsPerMinute);
                };
            }
            else if(config.useHypeRate || config.useStromno && !config.useBluetooth)
            {
                if (config.HypeRate_or_Stromno_SessionID != "")
                {
                    if (config.useHypeRate)
                    {
                        service = "HypeRate";
                    }
                    else if (config.useStromno)
                    {
                        service = "Stromno";
                    }
                    
                    Console.WriteLine("> [Service] " + service);
                    
                    _hypeRateStromnoHeartRateStromno = new HypeRate_Stromno_Service();
               
                    _hypeRateStromnoHeartRateStromno.HypeRate_Stromno_Updated += heart => 
                    {
                        hypeRate_Stromno_Reading = heart;
                
                        Console.Write($"{DateTime.Now}        \nService: (" + service + ")                       \n                               \n[HeartRate] " + hypeRate_Stromno_Reading + " BPM               \n                                 ");
                        Console.SetCursorPosition(0, 0);

                        lastUpdate = DateTime.Now;

                        if (config.putToFile)
                        {
                            File.WriteAllText("rate.txt", $"{hypeRate_Stromno_Reading}");
                        }

                        osc?.Update(hypeRate_Stromno_Reading);
                    };
                }
                else
                {
                    Console.Write("HypeRate_or_Stromno_SessionID is not defined");
                    Thread.Sleep(5000);
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.Write("Service not selected or Multiple Services selected");
                Thread.Sleep(5000);
                Environment.Exit(1);
            }

            if (config.serviceRefreshDelay < 2)
            {
                config.serviceRefreshDelay = 2;
                Console.Write("It is not recommended to update output less than every 2 seconds. (outside OSC par. and writeToFile)");
            }
            
            //Console
            Console.SetCursorPosition(0, 0);

            //Timed Functions
            while (true) 
            {
                //Timeout
                if (DateTime.Now - lastUpdate > TimeSpan.FromSeconds(config.TimeOutInterval)) 
                {
                    Debug.WriteLine("HeartRate monitor uninitialized. Starting...");
                    
                    string APIkey = null;
                    if (config.useBluetooth)
                    {
                        while(true) 
                        {
                            try {
                                heartrate.InitiateDefault();
                                break;
                            } catch (Exception e) {
                                Debug.WriteLine($"Failure while initiating Bluetooth service, retrying in {config.RestartDelay} seconds:");
                                Debug.WriteLine(e);
                                Thread.Sleep((int)(config.RestartDelay * 1000));
                            }
                        }
                    }
                    else if(config.useHypeRate || config.useStromno && !_hypeRateStromnoHeartRateStromno.IsConnected)
                    {
                        APIkey = null;
                        if (config.HypeRateAPIkey != "")
                        {
                            APIkey = config.HypeRateAPIkey;
                        }
                        _hypeRateStromnoHeartRateStromno.Init(config.HypeRate_or_Stromno_SessionID, service, APIkey);

                        while (true)
                        {
                            if (_hypeRateStromnoHeartRateStromno.IsConnected)
                            {
                                break;
                            }
                            
                            Thread.Sleep((int)(config.RestartDelay * 1000));
                        }
                    }
                }

                int beats = 0;
                if (config.useBluetooth)
                {
                    beats = reading.BeatsPerMinute;
                }

                if (config.useHypeRate || config.useStromno)
                {
                    beats = hypeRate_Stromno_Reading;
                }
                
                //Outputs
                discord?.SetPresence(new RichPresence
                {
                    Details = config.discordGameDesc,
                    State = $"{beats} BPM",
                });

                OSCChatBox.Updated(beats);

                Thread.Sleep(config.serviceRefreshDelay * 1000);
                
            }
        }
    }
}
