using IotGui.Models;
using Microsoft.Extensions.Hosting;
using Net.TcpServer;
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
        //private TcpListener serverSocket;

        public TcpService(IDataService dataService)
        {
            _dataService = dataService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                TcpServer tcpServer = new TcpServer(IPAddress.Any, 3333);
                while (true && !stoppingToken.IsCancellationRequested)
                {
                    string jsonString = string.Empty;
                    tcpServer.Start(_ =>
                    {

                        _.OnAccept = client =>
                        {
                            Debug.WriteLine($"OnAccept: {client}");
                        };
                        _.OnReceive = (client, data) =>
                        {
                            jsonString += Encoding.UTF8.GetString(data);
                            Debug.WriteLine($"OnReceive: {client} {Encoding.UTF8.GetString(data)}");
                        };
                        _.OnError = (client, ex) =>
                        {
                            Debug.WriteLine($"OnError: {client} {ex.Message}");
                        };
                        _.OnClose = (client, isCloseByClient) =>
                        {
                            Debug.WriteLine($"Result: {jsonString}");
                            try
                            {
                                var measurement = JsonConvert.DeserializeObject<MeasurementViewModel>(jsonString);
                                jsonString = string.Empty;
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
                            catch (Exception e)
                            {
                                jsonString = string.Empty;
                                Console.WriteLine("Exception: {0}", e);
                            }
                            Debug.WriteLine($"OnClose: {client} {(isCloseByClient ? "by client" : "by server")}");
                        };
                    });
                }
                Console.ReadKey();
                tcpServer.Stop();
            }).Start();
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
