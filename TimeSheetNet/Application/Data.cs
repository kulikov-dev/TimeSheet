using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeSheetNet.Application
{
    public class Data
    {
        public string Department { get; set; }
        public List<Employee> Employers { get; set; }
        public string Leader { get; set; }
        public string Position { get; set; }
    }

    public class Employee
    {
        public string FullName { get; set; }
        public int Hours { get; set; }
    }
}
