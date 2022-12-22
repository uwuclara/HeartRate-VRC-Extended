﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace HRPresence
{
    
    //Straight up stolen and edited. Use 200Tigersbloxed's proxy at ur own risk if u don't define API key.
    //https://github.com/200Tigersbloxed/HRtoVRChat_OSC/blob/main/HRtoVRChat_OSC/HRManagers/HypeRateManager.cs
    
    public class HypeRate_Stromno_Service
    {
        public event HypeRate_Stromno_UpdateEventHandler HypeRate_Stromno_Updated;
        public static int BeatsPerMinute { get; set; }
        public delegate void HypeRate_Stromno_UpdateEventHandler(int BeatsPerMinute);
        private WebsocketTemplate wst;
        private Thread _thread;
        private CancellationTokenSource tokenSource = new CancellationTokenSource();

        public bool IsConnected
        {
            get
            {
                if (wst != null)
                {
                    return wst.IsAlive;
                }
                return false;
            }
        }

        public void Init(string id, string service, string APIkey = null)
        {
            tokenSource = new CancellationTokenSource();
            StartThread(id, service, APIkey);
        }

        private async void HandleMessage(string message)
        {
            try
            {
                JObject jo = JObject.Parse(message);
                if (jo["method"] != null)
                {
                    string pingId = jo["pingId"]?.Value<string>();
                    await wst.SendMessage("{\"method\": \"pong\", \"pingId\": \"" + pingId + "\"}");
                }
                else
                {
                    HypeRate_Stromno_Updated?.Invoke(Convert.ToInt32(jo["hr"].Value<string>()));
                }
            }
            catch (Exception) { }
        }

        private void StartThread(string id, string service, string APIkey = null)
        {
            _thread = new Thread(async () =>
            {
                var reader = "";
                if (service == "HypeRate")
                {
                    reader = "hyperate";
                }
                else if (service == "Stromno")
                {
                    reader = "pulsoid";
                }
                
                var url = "wss://hrproxy.fortnite.lol:2096/hrproxy";
                
                if (APIkey != null && service == "HypeRate")
                {
                    url = "wss://app.hyperate.io/socket/websocket?token=" + APIkey;
                }
                
                wst = new WebsocketTemplate(url);
                
                var noerror = true;
                try
                {
                    await wst.Start();
                }
                catch(Exception e)
                {
                    Console.WriteLine("[HypeRate/Stromno Service] Failed to connect to HypeRate/Stromno server!" + e.Message);
                    noerror = false;
                }
                if (noerror)
                {
                    await wst.SendMessage("{\"reader\": \"" + reader + "\", \"identifier\": \"" + id + "\", \"service\": \"vrchat\"}");
                    while (!tokenSource.IsCancellationRequested)
                    {
                        if (IsConnected)
                        {
                            string message = await wst.ReceiveMessage();
                            if (!string.IsNullOrEmpty(message))
                            {
                                HandleMessage(message);
                            }
                        }
                        
                        Thread.Sleep(1);
                    }
                }
                await Close();
            });
            _thread.Start();
        }

        private async Task Close()
        {
            if (wst != null)
                if (wst.IsAlive)
                    try
                    {
                        await wst.Stop();
                        wst = null;
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("[HypeRate/Stromno Service] Failed to close connection to HypeRate Server! Exception: " + e.Message);
                    }
                else
                    Console.WriteLine("[HypeRate/Stromno Service] WebSocket is not alive! Did you mean to Dispose()?");
            else
                Console.WriteLine("[HypeRate/Stromno Service] WebSocket is null! Did you mean to Initialize()?");
        }

    }
    
    public class WebsocketTemplate
    {
        private string wsUri;
        private ClientWebSocket cws;

        public bool IsAlive => cws?.State == WebSocketState.Open;
        
        public WebsocketTemplate(string wsUri)
        {
            this.wsUri = wsUri;
        }

        public async Task Start()
        {
            bool noerror = false;
            try
            {
                if (cws == null)
                    cws = new ClientWebSocket();
                await cws.ConnectAsync(new Uri(wsUri), CancellationToken.None);
            }
            catch(Exception e)
            {
                Console.WriteLine("[HypeRate/Stromno Service] Failed to connect to WebSocket server! Exception: " + e.Message);
            }
        }

        private int senderror;
        public async Task SendMessage(string message, bool closeonfail = true)
        {
            if (cws != null)
            {
                if (cws.State == WebSocketState.Open)
                {
                    byte[] sendBody = Encoding.UTF8.GetBytes(message);
                    try
                    {
                        await cws.SendAsync(new ArraySegment<byte>(sendBody), WebSocketMessageType.Text, true, CancellationToken.None);
                        senderror = 0;
                    }
                    catch (Exception e) 
                    {
                        senderror++;
                        if (senderror > 15 && closeonfail)
                        {
                            await Stop();
                        }
                    }
                }
            }
        }

        private int receiveerror;
        public async Task<string> ReceiveMessage(bool closeonfail = true)
        {
            var clientbuffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = null;
            try
            {
                result = await cws.ReceiveAsync(clientbuffer, CancellationToken.None);
            }
            catch(Exception e)
            {
                receiveerror++;
                if (receiveerror > 15 && closeonfail)
                {
                    await Stop();
                }
            }
            
            if(result != null)
                if (result.Count != 0 || result.CloseStatus == WebSocketCloseStatus.Empty)
                {
                    string msg = Encoding.ASCII.GetString(clientbuffer.Array ?? Array.Empty<byte>());
                    return msg;
                }
            return String.Empty;
        }

        public async Task Stop()
        {
            if (cws != null)
                if (cws.State == WebSocketState.Open)
                    try
                    {
                        await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        cws.Dispose();
                        cws = null;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[HypeRate/Stromno Service] Failed to close connection to WebSocket Server!" + e.Message);
                    }
        }
    }
}