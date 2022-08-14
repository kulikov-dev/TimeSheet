namespace TimeSheet.Data
{
    /// <summary>
    /// Year information about holidays/weekends by months
    /// </summary>
    internal class AnnualWeekendsInfo
    {
        /// <summary>
        /// Data year
        /// </summary>
        internal int Year;

        /// <summary>
        /// Dictionary month - days off
        /// </summary>
        private readonly Dictionary<int, List<int>> _monthWeekends = new();

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="year"> Data year </param>
        internal AnnualWeekendsInfo(int year)
        {
            Year = year;
        }

        /// <summary>
        /// Get all weekends in month
        /// </summary>
        /// <param name="date"> Month date </param>
        /// <returns> List of month weekends  </returns>
        internal List<int> GetMonthWeekends(DateTime date)
        {
            return _monthWeekends.ContainsKey(date.Month) ? _monthWeekends[date.Month] : new List<int>();
        }

        /// <summary>
        /// Add weekends by month
        /// </summary>
        /// <param name="month"> Month </param>
        /// <param name="weekends"> List of weekends </param>
        internal void AddMonthWeekends(int month, List<int> weekends)
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
    }
}