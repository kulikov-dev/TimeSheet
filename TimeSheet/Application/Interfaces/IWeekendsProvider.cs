using TimeSheet.Application.Data;

namespace TimeSheet.Application.Interfaces
{
    internal interface IWeekendsProvider
    {
        Task<AnnualWeekendsInfo> GetAnnualWeekends(DateTime date);
    }
}
