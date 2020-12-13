using IotGui.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IotGui.Data
{
    public class DataService : IDataService
    {
        public List<MeasurementViewModel> GetData()
        {
            using (StreamReader reader = new StreamReader(@"MeasurementsData/example.json"))
            {
                string json = reader.ReadToEnd();
                var measurements = JsonConvert.DeserializeObject<List<MeasurementViewModel>>(json);
                return measurements;
            }
        }
    }
}
