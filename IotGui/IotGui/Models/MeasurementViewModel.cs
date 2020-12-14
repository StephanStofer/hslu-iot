using System.Collections.Generic;

namespace IotGui.Models
{
    public class MeasurementViewModel
    {
        public string timestamp { get; set; }
        public int water_0 { get; set; }
        public List<int> piezo_0 { get; set; }
        public List<int> piezo_1 { get; set; }
        public string date { get; set; }
        public string time { get; set; }
    }
}
