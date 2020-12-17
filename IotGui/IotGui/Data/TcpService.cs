using IotGui.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Net.TcpServer;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IotGui.Data
{
    public class TcpService : BackgroundService
    {
        private IDataService _dataService;
        private readonly IConfiguration _configuration;
        private readonly IMailService _mailService;
        private Dictionary<string, int> dict = new Dictionary<string, int> {{"piezo_0", 0},{"piezo_1",0}};

        

        //private TcpListener serverSocket;

        public TcpService(IDataService dataService, IConfiguration configuration, IMailService mailService)
        {
            _dataService = dataService;
            _configuration = configuration;
            _mailService = mailService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                TcpServer tcpServer = new TcpServer(IPAddress.Parse(_configuration["Host"]), int.Parse(_configuration["Port"]));
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
                                if(measurement.water_0 > int.Parse(_configuration["HumidityAlert"]))
                                {
                                    if(measurementsJson.TakeLast(3).All(mes => mes.water_0 > int.Parse(_configuration["HumidityAlert"])))
                                    {
                                        _mailService.SendAlertMail("water_0", measurement.water_0.ToString());
                                    }
                                }
                                if (calcDiff(measurement.piezo_0, "piezo_0").Average() > int.Parse(_configuration["PiezosAlert"]) ||
                                    calcDiff(measurement.piezo_1, "piezo_1").Average() > int.Parse(_configuration["PiezosAlert"]))
                                {
                                    _mailService.SendAlertMail("piezo_0", string.Empty);
                                }
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

        private List<int> calcDiff(List<int> data, string dictIndex)
        {
            List<int> diff1 = new List<int>();
            List<int> diff2 = new List<int>();
            
            
            for (int i = 1; i < data.Count; i++)
            {
                if (data[i] + dict[dictIndex] > 4095)
                {
                    dict[dictIndex] -= 1;
                }
                else
                {
                    dict[dictIndex] += 1;
                }
                data[i] += dict[dictIndex] - 4095;
                
                diff1.Add(data[i] - data[i - 1]);
            }

            for (int i = 1; i < diff1.Count; i++)
            {
                diff2.Add(diff1[i] - diff1[i - 1]);
            }

            return diff2;
        }
    }
}