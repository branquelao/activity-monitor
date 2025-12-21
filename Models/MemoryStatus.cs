using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityMonitor.Models
{
    public record MemoryStatus
    (
    ulong Total,
    ulong Used,
    double Percent
    );
}
