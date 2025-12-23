using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ActivityMonitor.Models
{
    public class ProcessInfo
    {
        public string Name { get; set; }
        public double Cpu { get; set; }
        public double Memory { get; set; }
    }

}
