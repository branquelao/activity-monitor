using ActivityMonitor.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityMonitor.Services
{
    public class ProcessService
    {
        public List<ProcessInfo> GetProcesses()
        {
            var processes = Process.GetProcesses();
            var list = new List<ProcessInfo>();

            foreach (var p in processes)
            {
                try
                {
                    list.Add(new ProcessInfo
                    {
                        Name = p.ProcessName,
                        Cpu = 0, // vamos calcular depois
                        Memory = Math.Round(p.WorkingSet64 / 1024.0 / 1024.0, 2)
                    });
                }
                catch
                {
                    // alguns processos não permitem acesso
                }
            }

            return list.OrderByDescending(p => p.Memory).ToList();
        }
    }
}
