using TimeSheet.Application.Interfaces;
using TimeSheet.Data;

namespace TimeSheet.Domain
{
    /// <summary>
    /// Storage of weekends
    /// </summary>
    internal sealed class WeekendsStorage
    {
        /// <summary>
        /// Annual weekends information
        /// </summary>
        private AnnualWeekendsInfo? _weekendsInfo;

        /// <summary>
        /// Weekends provider
        /// </summary>
        private readonly IWeekendsProvider _provider;

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="provider"> Weekends provider </param>
        internal WeekendsStorage(IWeekendsProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Load weekends from data provider
        /// </summary>
        /// <param name="yearDate"> Date with year </param>
        /// <returns> Task </returns>
        private async Task LoadWeekendsFromProvider(DateTime yearDate)
        {
            _weekendsInfo = await _provider.GetAnnualWeekends(yearDate);
        }

        /// <summary>
        /// Get weekends in month
        /// </summary>
        /// <param name="monthDate"> Date with month </param>
        /// <returns></returns>
        internal async Task<List<int>> GetMonthWeekends(DateTime monthDate)
        {
            if (_weekendsInfo == null || _weekendsInfo.Year != monthDate.Year)
            {
                await LoadWeekendsFromProvider(monthDate);
            }

            return _weekendsInfo?.GetMonthWeekends(monthDate) ?? new List<int>();
        }
    }
}