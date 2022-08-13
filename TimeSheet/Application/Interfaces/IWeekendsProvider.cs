using TimeSheet.Data;

namespace TimeSheet.Application.Interfaces
{
    /// <summary>
    /// Interface for holidays/weekends provider
    /// </summary>
    internal interface IWeekendsProvider
    {
        /// <summary>
        /// Get annual weekends
        /// </summary>
        /// <param name="date"> Date </param>
        /// <returns> Annual weekends </returns>
        Task<AnnualWeekendsInfo> GetAnnualWeekends(DateTime date);
    }
}