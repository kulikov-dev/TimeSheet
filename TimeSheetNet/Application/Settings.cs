using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetNet.Application
{
    internal static class Settings
    {
        public static readonly string DefaultWorkersPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "workers.json");

        public static string WorkersPath = DefaultWorkersPath;
    }
}
