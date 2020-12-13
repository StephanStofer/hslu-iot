using IotGui.Models;
using System.Collections.Generic;

namespace IotGui.Data
{
    public interface IDataService
    {
        List<MeasurementViewModel> GetData();
    }
}