using TimeSheet.Application.Data;
using TimeSheet.Application.Interfaces;

namespace TimeSheet.Application
{
    internal class WeekendsStorage
    {
        private AnnualWeekendsInfo? _weekendsInfo;
        private readonly IWeekendsProvider _provider;

        public WeekendsStorage(IWeekendsProvider provider)
        {
            _provider = provider;
        }

        private async Task LoadData(DateTime date)
        {
            _weekendsInfo = await _provider.GetAnnualWeekends(date);
        }

        public async Task<List<int>> GetMonthWeekends(DateTime date)
        {
            if (_weekendsInfo == null || _weekendsInfo.Year != date.Year)
            {
                await LoadData(date);
            }

            return _weekendsInfo?.GetMonthWeekends(date) ?? new List<int>();
        }
    }
}
