using System.Collections.Generic;

namespace IotGui.Models
{
    public class PiezosViewModel
    {
        public List<int> Piezo1 { get; private set; }
        public List<int> Piezo2 { get; private set; }
        public List<int> indexPiezo1 { get; private set; }
        public List<int> indexPiezo2 { get; private set; }
        public PiezosViewModel(List<int> p1_diff, List<int> p2_diff)
        {
            Piezo1 = p1_diff;
            Piezo2 = p2_diff;
            for (int i = 0; i <= p1_diff.Count; i++)
            {
                indexPiezo1.Add(i);
            }
            for (int i = 0; i <= p2_diff.Count; i++)
            {
                indexPiezo2.Add(i);
            }
        }
    }
}
