using System.Text.Json;
using Castle.Components.DictionaryAdapter;
using Moq;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;
using TimeSheet.Domain;

namespace TimeSheet.Tests
{
    /// <summary>
    /// Department info tests
    /// </summary>
    public class DepartmentStorageTests
    {
        /// <summary>
        /// Mock department provider
        /// </summary>
        private readonly Mock<IDepartmentProvider> _mockProvider;

        /// <summary>
        /// Expected department info
        /// </summary>
        private readonly DepartmentInfo _expectedDepartmentInfo = new()
        {
            DepartmentTitle = "Отдел разработки ПО",
            LeaderName = "Иванов И.И.",
            LeaderPosition = "Начальник отдела",
            Employees = new EditableList<Employee>()
            {
                new(fullName: "Петров П.П.", workHours: 8),
                new(fullName: "Васечкин В.В.", workHours: 6)
            }
        };

        /// <summary>
        /// Setup
        /// </summary>
        public DepartmentStorageTests()
        {
            _mockProvider = new Mock<IDepartmentProvider>();
            _mockProvider.Setup(item => item.Load()).Returns(Task.FromResult(_expectedDepartmentInfo)!);
        }

        [Fact]
        public async void TestProvider()
        {
            var result = await _mockProvider.Object.Load();
            Assert.NotNull(result);
            Assert.Equal(_expectedDepartmentInfo, result);
        }

        [Fact]
        public async void TestLoading()
        {
            await using var fileStream = new MemoryStream(Resource.department);
            var result = await JsonSerializer.DeserializeAsync<DepartmentInfo>(fileStream);

            Assert.NotNull(result);
            Assert.Equal(_expectedDepartmentInfo, result);
        }

        [Fact]
        public async void TestStorage()
        {
            var storage = new DepartmentStorage(_mockProvider.Object);
            await storage.Load();

            Assert.NotNull(storage);
            var index = 0;
            foreach (var employee in storage)
            {
                Assert.True(employee.Equals(_expectedDepartmentInfo.Employees[index]));
                ++index;
            }

            Assert.True(storage.HasEmployees);
            Assert.Equal(_expectedDepartmentInfo.Employees.Count, storage.EmployeesCount);
            Assert.Equal(_expectedDepartmentInfo.LeaderName, storage.LeaderName);
            Assert.Equal(_expectedDepartmentInfo.LeaderPosition, storage.LeaderPosition);
            Assert.Equal(_expectedDepartmentInfo.DepartmentTitle, storage.DepartmentTitle);
        }

    }
}
