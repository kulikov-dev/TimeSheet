using System.Collections;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;

namespace TimeSheet.Domain
{
    /// <summary>
    /// Storage of department info: workers, leader, etc
    /// </summary>
    internal class DepartmentStorage : IEnumerable<Employee>
    {
        /// <summary>
        /// Weekends provider
        /// </summary>
        private readonly IDepartmentProvider _provider;



        private DepartmentInfo? _departmentInfo;

        internal DepartmentStorage(IDepartmentProvider provider)
        {
            _provider = provider;
        }

        internal async Task Load()
        {
            _departmentInfo = await _provider.Load();
        }

        internal bool HasWorkers => _departmentInfo != null && _departmentInfo.Employers.Count > 0;

        internal int WorkersCount => _departmentInfo == null ? 0 : _departmentInfo.Employers.Count;

        internal string Department => _departmentInfo?.Department ?? string.Empty;

        internal string Position => _departmentInfo?.Position ?? string.Empty;

        internal string Leader => _departmentInfo?.Leader ?? string.Empty;

        public IEnumerator<Employee> GetEnumerator()
        {
            return _departmentInfo != null ? _departmentInfo.Employers.GetEnumerator() : Enumerable.Empty<Employee>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
