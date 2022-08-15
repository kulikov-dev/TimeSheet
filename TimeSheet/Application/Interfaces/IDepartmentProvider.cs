using TimeSheet.Data;

namespace TimeSheet.Application.Interfaces
{
    /// <summary>
    /// Interface for department info data sources
    /// </summary>
    public interface IDepartmentProvider
    {
        /// <summary>
        /// Load department information
        /// </summary>
        /// <returns> Department information </returns>
        Task<DepartmentInfo?> Load();
    }
}