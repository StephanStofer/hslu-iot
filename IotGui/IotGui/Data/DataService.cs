using IotGui.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace IotGui.Data
{
    public class DataService : IDataService
    {
        private Dictionary<string, int> dict = new Dictionary<string, int> {{"piezo_0", 0}, {"piezo_1", 0}};

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


        public List<int> CalcDiff(List<int> data, string dictionaryIndex)
        {
            List<int> diff1 = new List<int>();
            List<int> diff2 = new List<int>();

            for (int i = 1; i < data.Count; i++)
            {
                if (data[i] + dict[dictionaryIndex] > 4095)
                {
                    dict[dictionaryIndex] -= 1;
                }
                else
                {
                    dict[dictionaryIndex] += 1;
                }

                data[i] += dict[dictionaryIndex] - 4095;

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