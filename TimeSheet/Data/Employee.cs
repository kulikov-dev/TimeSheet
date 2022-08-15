namespace TimeSheet.Data
{
    /// <summary>
    /// Employee info
    /// </summary>
    public struct Employee : IEquatable<Employee>
    {
        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="fullName"> Employee name </param>
        /// <param name="workHours"> Work hours </param>
        public Employee(string fullName, int workHours)
        {
            FullName = fullName;
            WorkHours = workHours;
        }

        /// <summary>
        /// Full name
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Work hours
        /// </summary>
        public int WorkHours { get; set; }

        /// <summary>
        /// Check objects for equals
        /// </summary>
        /// <param name="other"> Second object </param>
        /// <returns> Flag if are equals </returns>
        public bool Equals(Employee other)
        {
            return FullName == other.FullName && WorkHours == other.WorkHours;
        }
    }
}