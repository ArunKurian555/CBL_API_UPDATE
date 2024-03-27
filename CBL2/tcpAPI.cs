using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace CBL2
{

    public class TCPServer
    {


        public Config.Configuration config = new Config.Configuration();

        NetworkStream stream;
        public bool activeConnection = false;
        private string _apiRequest;
        public string apiRequest
        {
            get
            {
                OnSettingsChanged();
                return _apiRequest;
            }
            set
            {
                OnSettingsChanged();
                if (_apiRequest != value)
                {
                    _apiRequest = value;
                }
            }
        }
        public delegate void apiRequestChangedEventHandler();
        public event apiRequestChangedEventHandler apiRequestChanged;

        protected virtual void OnSettingsChanged()
        {
            apiRequestChanged?.Invoke();
        }

        public void SendData()
        {
            if (activeConnection)
            {
                string jsonData = JsonConvert.SerializeObject(config);
                byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonData);
                byte[] newlineBytes = Encoding.UTF8.GetBytes("\n");
                try
                {
                stream.Write(jsonBytes, 0, jsonBytes.Length);
                stream.Write(newlineBytes, 0, newlineBytes.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error: {e.Message}");

                }
            }
        }
        public async Task StartServerAsync()
        {
            string host = "0.0.0.0"; 
            int port = 41798; 

            TcpListener server = null;
        
            try
            {
                server = new TcpListener(IPAddress.Parse(host), port);

                // Start listening for client requests
                server.Start();

                Console.WriteLine("Server started. Waiting for connections...");

                // Enter the listening loop
                while (true)
                {
                    // Perform a non-blocking call to accept requests
                    TcpClient client = await server.AcceptTcpClientAsync();
                    Console.WriteLine("Client connected.");
                    stream = client.GetStream();
                    activeConnection = true;
                    SendData();


                    // Handle client communication asynchronously
                    _ = HandleClientAsync(client , stream);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error: {e.Message}");
            }
            finally
            {
                server?.Stop();
                activeConnection = false;
            }
        }

        async Task HandleClientAsync(TcpClient client, NetworkStream stream)
        {

            byte[] buffer = new byte[1024];
            
            int bytesRead;

            try
            {
                StringBuilder receivedData = new StringBuilder();

                while (client.Connected)
                {

                    try
                    {
                        // Read data into buffer
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

                        // Convert received bytes to string
                        string receivedChunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                        // Append received chunk to the StringBuilder
                        receivedData.Append(receivedChunk);

                        // Check if the received data contains the delimiter "\n"
                        if (receivedChunk.Contains("\n"))
                        {

                            int lastIndex = receivedData.ToString().LastIndexOf('\n');
                            string jsonData = receivedData.ToString(0, lastIndex + 1);

                            receivedData.Remove(0, lastIndex + 1);
                            // Deserialize JSON into Configuration object
                            config = JsonConvert.DeserializeObject<Config.Configuration>(jsonData);

                            apiRequest = " ";

                            Array.Clear(buffer, 0, buffer.Length);
                        }
                    }
                    catch 
                    {
                        Console.WriteLine("Failed to deserialize JSON: ");
                        continue;
                    }

                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error handling client: {e.Message}");
            }
            finally
            {
                stream.Close();

                Console.WriteLine("Client disconnected.");
            }
        }

    }
}
