using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.WebSockets;
using System.Text;

namespace HRPresence
{
    //Straight up stolen and edited. Use 200Tigersbloxed's proxy at ur own risk if u don't define API key.
    //https://github.com/200Tigersbloxed/HRtoVRChat_OSC/blob/main/HRtoVRChat_OSC/HRManagers/HypeRateManager.cs
    public class HypeRate_Stromno_Service
    {
        public event HypeRate_Stromno_UpdateEventHandler HypeRate_Stromno_Updated;
        public delegate void HypeRate_Stromno_UpdateEventHandler(int BeatsPerMinute);
        private WebsocketTemplate? wst;
        private Thread? _thread;
        public bool IsActive() => IsConnected;

        private bool IsConnected
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

        public bool Init(string id)
        {
            StartThread(id);
            Console.WriteLine("Initialized WebSocket!");
            return IsConnected;
        }

        private async void HandleMessage(string message)
        {
            try
            {
                // Parse the message and get the HR or Pong
                var jo = JObject.Parse(message);
                
                if (jo["method"] != null)
                {
                    var pingId = jo["pingId"]?.Value<string>();
                    await wst.SendMessage("{\"method\": \"pong\", \"pingId\": \"" + pingId + "\"}");
                }
                else
                {
                    
                    HypeRate_Stromno_Updated?.Invoke(Convert.ToInt32(jo["hr"].Value<string>()));
                }
            }
            catch (Exception) { }
        }

        public void StartThread(string id)
        {
            _thread = new Thread(async () =>
            {
                wst = new WebsocketTemplate("wss://hrproxy.fortnite.lol:2096/hrproxy");
                var noerror = true;
                
                try
                {
                    await wst.Start();
                }
                catch(Exception e)
                {
                    Console.WriteLine("Failed to connect to HypeRate server!", e);
                    noerror = false;
                }
                if (noerror)
                {
                    await wst.SendMessage("{\"reader\": \"hyperate\", \"identifier\": \"" + id + "\", \"service\": \"vrchat\"}");

                    while (IsConnected)
                    {
                        var message = await wst.ReceiveMessage();
                        if (!string.IsNullOrEmpty(message))
                            HandleMessage(message);
                        
                        Thread.Sleep(1);
                    }
                    
                    await Close();
                    Console.WriteLine("here Closed HypeRate");
                }
                else
                {
                    await Close();
                    Console.WriteLine("hete Closed HypeRate");
                }
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
                        Console.WriteLine("Failed to close connection to HypeRate Server! Exception: ", e);
                    }
                else
                    Console.WriteLine("WebSocket is not alive! Did you mean to Dispose()?");
            else
                Console.WriteLine("WebSocket is null! Did you mean to Initialize()?");
        }
    }
    
    public class WebsocketTemplate
    {
        public string wsUri;
        private ClientWebSocket cws;

        public bool IsAlive => cws?.State == WebSocketState.Open;
        
        public WebsocketTemplate(string wsUri)
        {
            this.wsUri = wsUri;
        }

        public async Task<bool> Start()
        {
            bool noerror = false;
            try
            {
                if (cws == null)
                    cws = new ClientWebSocket();
                await cws.ConnectAsync(new Uri(wsUri), CancellationToken.None);
                noerror = true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Failed to connect to WebSocket server! Exception: ", e);
            }

            return noerror;
        }

        private int senderror = 0;
        public async Task SendMessage(string message, bool closeonfail = true)
        {
            if (cws != null)
            {
                if (cws.State == WebSocketState.Open)
                {
                    var sendBody = Encoding.UTF8.GetBytes(message);
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
            
            // Only check if result is not null
            if (result != null)
            {
                if (result.Count != 0 || result.CloseStatus == WebSocketCloseStatus.Empty)
                {
                    string msg = Encoding.ASCII.GetString(clientbuffer.Array ?? Array.Empty<byte>());
                    return msg;
                }
            }
                
            return String.Empty;
        }

        public async Task<bool> Stop()
        {
            if (cws != null)
            {
                if (cws.State == WebSocketState.Open)
                {
                    try
                    {
                        await cws.CloseAsync(WebSocketCloseStatus.NormalClosure, String.Empty, CancellationToken.None);
                        cws.Dispose();
                        cws = null;
                        return true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Failed to close connection to WebSocket Server!", e);
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
                
            return false;
        }
    }
}
    