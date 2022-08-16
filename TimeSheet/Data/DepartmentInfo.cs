namespace TimeSheet.Data
{
    /// <summary>
    /// Information about a department
    /// </summary>
    public class DepartmentInfo : IEquatable<DepartmentInfo>
    {
        /// <summary>
        /// Department title
        /// </summary>
        public string DepartmentTitle { get; set; } = string.Empty;

        /// <summary>
        /// Department leader name
        /// </summary>
        public string LeaderName { get; set; } = string.Empty;

        /// <summary>
        /// Department leader position
        /// </summary>
        public string LeaderPosition { get; set; } = string.Empty;

        /// <summary>
        /// List of employees
        /// </summary>
        public List<Employee> Employees { get; set; } = new();

        /// <summary>
        /// Check objects for equals
        /// </summary>
        /// <param name="other"> Second object </param>
        /// <returns> Flag if are equals </returns>
        public bool Equals(DepartmentInfo? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return DepartmentTitle == other.DepartmentTitle && LeaderName == other.LeaderName && LeaderPosition == other.LeaderPosition && Employees.SequenceEqual(other.Employees);
        }

        /// <summary>
        /// Check objects for equals
        /// </summary>
        /// <param name="obj"> Second object </param>
        /// <returns> Flag if are equals </returns>
        public override bool Equals(object? obj)
        {
            return obj?.GetType() == GetType() && Equals((DepartmentInfo)obj);
        }

        /// <summary>
        /// Get object hash code
        /// </summary>
        /// <returns> Hash code </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(DepartmentTitle, LeaderName, LeaderPosition, Employees);
        }
    }
}