namespace TimeSheet.Application.Data
{
    public class DepartmentInfo
    {
        public string Department { get; set; }
        public List<Employee> Employers { get; set; }
        public string Leader { get; set; }
        public string Position { get; set; }
    }
}
