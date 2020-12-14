using IotGui.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace IotGui.Data
{
    public class DataService : IDataService
    {
        public List<MeasurementViewModel> GetData()
        {
            if (File.Exists(@"MeasurementsData/example.json"))
            {
                using (StreamReader reader = new StreamReader(@"MeasurementsData/example.json"))
                {
                    string json = reader.ReadToEnd();
                    var measurements = JsonConvert.DeserializeObject<List<MeasurementViewModel>>(json);
                    return measurements;
                }
            }
            else
            {
                File.Create(@"MeasurementsData/example.json");
                return null;
            }
        }
    }
}
