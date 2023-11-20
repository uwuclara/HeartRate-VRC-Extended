using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using DiscordRPC;
using HRPresence;
using HRPresence.Objects;
using Newtonsoft.Json;

namespace HeartRateApp
{
    public static class HeartRateApp
    {
        private static DateTime                 _lastBeatUpdate = DateTime.MinValue;
        private static DateTime                 _serviceUpdate  = DateTime.MinValue;
        private static Bluetooth_Service        _bluetoothService;
        private static HypeRate_Stromno_Service _hypeRateStromnoService;
        private static DiscordRpcClient         _discordRPC;
        private static Config                   _config;
        private static OSCChatBox               _chatBoxOsc;
        private static OSCParameters            _parametersOsc;
        private static string                   _selectedService;
        private static int                      _currentHeartRate;
        
        private static void Main(string[] args) => StartAsync().GetAwaiter().GetResult();
    
        private static async Task StartAsync()
        {
            //Window
            Console.CursorVisible = false;
            Console.WindowHeight = 8;
            Console.WindowWidth = 50;
            
            //Config
            if (File.Exists("config.json")) 
            {
                var json = File.ReadAllText("config.json");
                _config = JsonConvert.DeserializeObject<Config>(json);
            } 
            else
            { 
                _config = new Config();
                var json = JsonConvert.SerializeObject(_config, Formatting.Indented);
                File.WriteAllText("config.json", json);
            }
            
            //Initialize Outputs
            if (_config.output_discord_rpc)
            {
                _discordRPC = new DiscordRpcClient(_config.discord_rpcid);
                _discordRPC.Initialize();
                Console.WriteLine("> [Config] Discord RPC [on]");
            }

            if (_config.output_parameters_osc || _config.output_chat_osc)
            {
                var udpClient = new UdpClient();

                if (IPAddress.TryParse(_config.osc_address, out var ip))
                {
                    udpClient.Connect(ip, _config.osc_port);
                    
                    if (_config.output_chat_osc)
                    { 
                        _chatBoxOsc = new OSCChatBox(udpClient, _config.osc_chatBox_Message);
                        Console.WriteLine("> [Config] OSC ChatBox [on]");
                    }
                
                    if (_config.output_parameters_osc)
                    {
                        _parametersOsc = new OSCParameters(udpClient, _config.osc_heartrate_name_int, _config.osc_heartrate_name_float, _config.osc_heartrate_name_ones, _config.osc_heartrate_name_tens, _config.osc_heartrate_name_hundreds);
                        Console.WriteLine("> [Config] OSC Parameters [on]");
                    }
                }
                else
                {
                    Console.WriteLine("ERROR: IP Address in config is invalid!");
                }
            }

            if (_config.output_put_to_file)
            {
                Console.WriteLine("> [Config] PutToFile [on]");
            }
            
            
            //Services
            if(_config.input_bluetooth && !_config.input_hype_rate && !_config.input_stromno)
            {
                _selectedService = "Bluetooth";
                
                _bluetoothService = new Bluetooth_Service();
                _bluetoothService.HeartRateUpdated += heart => 
                {
                    _lastBeatUpdate = DateTime.Now;
                    _currentHeartRate = heart.BeatsPerMinute;
                    HeartBeatUpdate();
                };
            }
            else if(_config.input_hype_rate || _config.input_stromno && !_config.input_bluetooth)
            {
                if (_config.hyperate_or_stromno_session_id != "")
                {
                    if (_config.input_hype_rate)
                    {
                        _selectedService = "HypeRate";
                    }
                    else if (_config.input_stromno)
                    {
                        _selectedService = "Stromno";
                    }
                    
                    _hypeRateStromnoService = new HypeRate_Stromno_Service();
                    _hypeRateStromnoService.HypeRate_Stromno_Updated += heart => 
                    {
                        _lastBeatUpdate = DateTime.Now;
                        _currentHeartRate = heart;
                        HeartBeatUpdate();
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
            
            Console.WriteLine("> [Service] " + _selectedService);
            Console.WriteLine();
            
            if (_config.delay_serviceRefresh < 2)
            {
                _config.delay_serviceRefresh = 2;
                Console.Write("It is not recommended to update output less than every 2 seconds. (outside OSC par. and writeToFile)");
            }
            
            ConnectToService();
            
            var keepAliveTimer = new System.Timers.Timer(_config.delay_restart_input * 1000);
            keepAliveTimer.AutoReset = true;
            keepAliveTimer.Elapsed += KeepAliveTimer;
            keepAliveTimer.Start();
            
            await Task.Delay(-1);
        }

        private static void KeepAliveTimer(object? state, ElapsedEventArgs elapsedEventArgs)
        {
            if (_config.input_bluetooth && DateTime.Now - _lastBeatUpdate > TimeSpan.FromSeconds(_config.delay_restart_input))
            {
                ReconnectToService();
            }
            
            if ((_config.input_hype_rate || _config.input_stromno) && !_hypeRateStromnoService.IsActive())
            {
                ReconnectToService();
            }
        }

        private static void ReconnectToService()
        {
            Console.Clear();
            Console.WindowHeight = 8;
            Console.WindowWidth = 50;
            Console.WriteLine("Reconnecting to service...");
            ConnectToService();
        }
        
        private static void ConnectToService()
        {
            if (_config.input_bluetooth)
            {
                try 
                {
                    _bluetoothService.InitiateDefault();
                } 
                catch (Exception e) { }
            }
            else if(_config.input_hype_rate || _config.input_stromno)
            { 
                try
                {
                    _hypeRateStromnoService.Init(_config.hyperate_or_stromno_session_id);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        private static bool _needClear = true;
        
        private static void HeartBeatUpdate()
        {
            Console.WindowHeight = 5;
            Console.WindowWidth = 32;

            if (_needClear)
            {
                _needClear = false;
                Console.Clear();
            }
            
            _lastBeatUpdate = DateTime.Now;
            
            Console.Write($"{DateTime.Now}        \nService: (" + _selectedService + ")                       \n                               \n[HeartRate] " + _currentHeartRate + " BPM               \n                                 ");
            Console.SetCursorPosition(0, 0);
            
            //Outputs
            if (_config.output_put_to_file)
                File.WriteAllText("currentHeartRate.txt", $"{_currentHeartRate}");
            
            _parametersOsc?.UpdateParameters(_currentHeartRate);
            
            if (DateTime.UtcNow <= _serviceUpdate) return;
            _serviceUpdate = DateTime.UtcNow.AddMilliseconds(_config.delay_serviceRefresh * 1000);
            
            _discordRPC?.SetPresence(new RichPresence
            {
                Details = _config.discord_game_desc,
                State = $"{_currentHeartRate} BPM"
            });

            _chatBoxOsc?.updateChatBox(_currentHeartRate);
        }
    }
}
