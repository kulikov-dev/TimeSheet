using System.Collections;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;

namespace TimeSheet.Domain
{
    /// <summary>
    /// Storage of department info: workers, leader, etc
    /// </summary>
    public sealed class DepartmentStorage : IEnumerable<Employee>
    {
        /// <summary>
        /// Weekends provider
        /// </summary>
        private readonly IDepartmentProvider _provider;

        /// <summary>
        /// Department information
        /// </summary>
        private DepartmentInfo? _departmentInfo;

        /// <summary>
        /// Constructor with parameters
        /// </summary>
        /// <param name="provider"> Data provider </param>
        public DepartmentStorage(IDepartmentProvider provider)
        {
            _provider = provider;
        }

        /// <summary>
        /// Check if a department has employees
        /// </summary>
        public bool HasEmployees => _departmentInfo?.Employees?.Count > 0;

        /// <summary>
        /// Employees count
        /// </summary>
        public int EmployeesCount => _departmentInfo == null ? 0 : _departmentInfo.Employees.Count;

        /// <summary>
        /// Department title
        /// </summary>
        public string DepartmentTitle => _departmentInfo?.DepartmentTitle ?? string.Empty;

        /// <summary>
        /// Department leader position
        /// </summary>
        public string LeaderPosition => _departmentInfo?.LeaderPosition ?? string.Empty;

        /// <summary>
        /// Department leader name
        /// </summary>
        public string LeaderName => _departmentInfo?.LeaderName ?? string.Empty;

        /// <summary>
        /// Enumerator
        /// </summary>
        /// <returns> List of employees </returns>
        public IEnumerator<Employee> GetEnumerator()
        {
            return _departmentInfo != null ? _departmentInfo.Employees.GetEnumerator() : Enumerable.Empty<Employee>().GetEnumerator();
        }

        /// <summary>
        /// Get enumerator
        /// </summary>
        /// <returns> Enumerator </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Load department info from a provider
        /// </summary>
        /// <returns> Task </returns>
        public async Task Load()
        {
            _departmentInfo = await _provider.Load();
        }
    }
}