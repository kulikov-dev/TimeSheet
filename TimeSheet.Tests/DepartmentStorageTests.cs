using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Components.DictionaryAdapter;
using Moq;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;
using TimeSheet.Domain;

namespace TimeSheet.Tests
{
    public class DepartmentStorageTests
    {
        private Mock<IDepartmentProvider> departmentProvider;
        private DepartmentStorage storage;

        private DepartmentInfo expectedDepartmentInfo = new()
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

        public DepartmentStorageTests()
        {
            departmentProvider = new Mock<IDepartmentProvider>();
            departmentProvider.Setup(item => item.Load()).Returns(Task.FromResult(expectedDepartmentInfo)!);

            storage = new DepartmentStorage(departmentProvider.Object);
        }

        [Fact]
        public async void TestProvider()
        {
            var result = await departmentProvider.Object.Load();
            Assert.NotNull(result);
            Assert.Equal(expectedDepartmentInfo, result);
        }

        [Fact]
        public void TestStorage()
        {
            Assert.NotNull(storage);
            var index = 0;
            foreach (var employee in storage)
            {
                Assert.True(employee.Equals(expectedDepartmentInfo.Employees[index]));
                ++index;
            }
        }
       
    }
}
