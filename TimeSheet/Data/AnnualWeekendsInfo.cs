namespace TimeSheet.Data
{
    /// <summary>
    /// Year information about holidays/weekends by months
    /// </summary>
    public class AnnualWeekendsInfo : IEquatable<AnnualWeekendsInfo>
    {
        /// <summary>
        /// Dictionary month - days off
        /// </summary>
        private readonly Dictionary<int, List<int>> _monthWeekends = new();

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="year"> Data year </param>
        public AnnualWeekendsInfo(int year)
        {
            Year = year;
        }

        /// <summary>
        /// Data year
        /// </summary>
        public int Year { get; }

        /// <summary>
        /// Get all weekends in month
        /// </summary>
        /// <param name="date"> Month date </param>
        /// <returns> List of month weekends  </returns>
        public List<int> GetMonthWeekends(DateTime date)
        {
            return _monthWeekends.ContainsKey(date.Month) ? _monthWeekends[date.Month] : new List<int>();
        }

        /// <summary>
        /// Add weekends by month
        /// </summary>
        /// <param name="month"> Month </param>
        /// <param name="weekends"> List of weekends </param>
        public void AddMonthWeekends(int month, List<int> weekends)
        {
            if (!_monthWeekends.ContainsKey(month))
            {
                _monthWeekends.Add(month, weekends);
            }
            else
            {
                _monthWeekends[month] = weekends;
            }
        }

        /// <summary>
        /// Check objects for equals
        /// </summary>
        /// <param name="other"> Second object </param>
        /// <returns> Flag if are equals </returns>
        public bool Equals(AnnualWeekendsInfo? other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }
            
            var isWeekendsEqual = _monthWeekends.Count == other._monthWeekends.Count && !_monthWeekends.Except(other._monthWeekends).Any();
            return isWeekendsEqual && Year == other.Year;
        }

        /// <summary>
        /// Check objects for equals
        /// </summary>
        /// <param name="obj"> Second object </param>
        /// <returns> Flag if are equals </returns>
        public override bool Equals(object? obj)
        {
            return obj?.GetType() == GetType() && Equals((AnnualWeekendsInfo)obj);
        }

        /// <summary>
        /// Get object hash code
        /// </summary>
        /// <returns> Hash code </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(_monthWeekends, Year);
        }
    }
}