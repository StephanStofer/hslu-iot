using IotGui.Models;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotGui.Data
{
    public class TcpService : BackgroundService
    {
        private IDataService _dataService;

        public TcpService(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            IPAddress iPAddress = IPAddress.Parse("0.0.0.0");
            TcpListener listener = new TcpListener(iPAddress, 3333);
            listener.Start();
            try
            {
                while (true)
                {
                    using (TcpClient client = await listener.AcceptTcpClientAsync())
                    {
                        if (client.Connected)
                        {
                            using (NetworkStream stream = client.GetStream())
                            {
                                Debug.Print($"Typ client started sucesfully on {iPAddress}\nconnection established: {client.Connected} | {client.Available}");
                                //while (!stoppingToken.IsCancellationRequested)
                                //{
                                    //if (listener.Pending()) break;
                                    int byteRead = 0;
                                    byte[] buffer = new byte[20000];
                                    string jsonString = string.Empty;
                                    do
                                    {
                                        byteRead = stream.Read(buffer, 0, 1000);
                                        jsonString += Encoding.ASCII.GetString(buffer, 0, byteRead);
                                    }
                                    while (byteRead > 0);
                                //Memory<byte> memory = new Memory<byte>(new byte[1000000]);
                                //int bytesRead = await stream.ReadAsync(memory, stoppingToken);
                                ////Debug.Print($"Read bytes {bytesRead}");
                                //var jsonString = Encoding.UTF8.GetString(memory.ToArray());
                                if (isValidJson(jsonString))
                                    {
                                        var measurement = JsonConvert.DeserializeObject<MeasurementViewModel>(jsonString);
                                        var measurementsJson = _dataService.GetData();
                                        if (measurement != null)
                                        {
                                            if (measurementsJson == null)
                                            {
                                                measurementsJson = new List<MeasurementViewModel>();
                                            }
                                            measurementsJson.Add(measurement);
                                            using (StreamWriter file = File.CreateText(@"MeasurementsData/example.json"))
                                            {
                                                measurement.date = DateTime.Now.ToShortDateString();
                                                measurement.time = DateTime.Now.ToLongTimeString();
                                                measurement.timestamp = DateTime.Now.ToLongDateString();
                                                JsonSerializer serializer = new JsonSerializer();
                                                serializer.Serialize(file, measurementsJson);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                
            }
            catch (Exception e)
            {
                Debug.Fail($"Failed to start TCP listener: {e.Message}");
            }
        }

        private bool isValidJson(string s)
        {
            try
            {
                s = s.TrimEnd(',');
                JToken.Parse(s);
                return true;
            }
            catch (JsonReaderException ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
