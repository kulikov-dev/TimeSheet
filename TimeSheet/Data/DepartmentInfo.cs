namespace TimeSheet.Data
{
    /// <summary>
    /// Information about a department
    /// </summary>
    internal class DepartmentInfo
    {
        /// <summary>
        /// Department title
        /// </summary>
        public string? DepartmentTitle { get; set; }

        /// <summary>
        /// Department leader name
        /// </summary>
        public string? LeaderName { get; set; }

        /// <summary>
        /// Department leader position
        /// </summary>
        public string? LeaderPosition { get; set; }

        /// <summary>
        /// List of employees
        /// </summary>
        public List<Employee>? Employees { get; set; }
    }
}