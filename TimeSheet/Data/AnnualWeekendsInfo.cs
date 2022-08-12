namespace TimeSheet.Application.Data
{
    internal class AnnualWeekendsInfo
    {
        private Dictionary<int, List<int>> monthWeekends = new Dictionary<int, List<int>>();
        public int Year;

        public AnnualWeekendsInfo(int year)
        {
            Year = year;
        }

        public List<int> GetMonthWeekends(DateTime date)
        {
            if (monthWeekends.ContainsKey(date.Month))
            {
                return monthWeekends[date.Month];
            }

            return new List<int>();
        }

        public void AddMonthWeekends(int month, List<int> weekends)
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

        public void AddMonthWeekends(DateTime date, List<int> weekends)
        {
            AddMonthWeekends(date.Month, weekends);
        }
    }
}
