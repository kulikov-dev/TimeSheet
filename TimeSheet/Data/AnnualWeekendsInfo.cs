namespace TimeSheet.Data
{
    internal class AnnualWeekendsInfo
    {
        private Dictionary<int, List<int>> monthWeekends = new Dictionary<int, List<int>>();
        internal int Year;

        internal AnnualWeekendsInfo(int year)
        {
            Year = year;
        }

        internal List<int> GetMonthWeekends(DateTime date)
        {
            if (monthWeekends.ContainsKey(date.Month))
            {
                return monthWeekends[date.Month];
            }

            return new List<int>();
        }

        internal void AddMonthWeekends(int month, List<int> weekends)
        {
            if (!monthWeekends.ContainsKey(month))
            {
                monthWeekends.Add(month, weekends);
            }
            else
            {
                monthWeekends[month] = weekends;
            }
        }

        internal void AddMonthWeekends(DateTime date, List<int> weekends)
        {
            AddMonthWeekends(date.Month, weekends);
        }
    }
}
