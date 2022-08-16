using System.Text.Json;
using Moq;
using OfficeOpenXml;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;
using TimeSheet.Domain;

namespace TimeSheet.Tests
{
    public class ReportCreatorTests
    {
        private readonly Mock<IDepartmentProvider> _departmentProvider;
        private readonly Mock<IWeekendsProvider> _weekendsProvider;
        private readonly DateTime _date = new(2021, 01, 01);

        public ReportCreatorTests()
        {
            _departmentProvider = new Mock<IDepartmentProvider>();
            _departmentProvider.Setup(item => item.Load()).Returns(async () =>
            {
                await using var fileStream = new MemoryStream(Resource.department);
                return await JsonSerializer.DeserializeAsync<DepartmentInfo>(fileStream);
            });

            var weekendsInfo = new AnnualWeekendsInfo(_date.Year);
            _weekendsProvider = new Mock<IWeekendsProvider>();
            _weekendsProvider.Setup(item => item.GetAnnualWeekends(_date)).Returns(Task.FromResult(weekendsInfo));
        }

        [Fact]
        public async void CheckReport()
        {
            var resultFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tests", "result.xlsx");
            var expectedFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tests", "expected.xlsx");
            if (File.Exists(resultFilePath))
            {
                File.Delete(resultFilePath);
            }

            if (!File.Exists(expectedFilePath))
            {
                await File.WriteAllBytesAsync(expectedFilePath, Resource.expected);
            }

            await ReportCreator.Create(_date, _departmentProvider.Object, _weekendsProvider.Object, exportPath: resultFilePath);

            using var expectedExcel = new ExcelPackage(new FileInfo(expectedFilePath));
            using var resultExcel = new ExcelPackage(new FileInfo(resultFilePath));

            Assert.True(resultExcel.Workbook.Worksheets.Count > 0);

            var expectedWorksheet = expectedExcel.Workbook.Worksheets[0];
            var resultWorksheet = resultExcel.Workbook.Worksheets[0];

            Assert.Equal(expectedWorksheet.Name, resultWorksheet.Name);
            for (var i = expectedWorksheet.Dimension.Start.Row; i <= expectedWorksheet.Dimension.End.Row; ++i)
            {
                for (var j = expectedWorksheet.Dimension.Start.Column; j <= expectedWorksheet.Dimension.End.Column; ++j)
                {
                    var expectedCell = expectedWorksheet.Cells[i, j];
                    var resultCell = resultWorksheet.Cells[i, j];
                    Assert.NotNull(resultCell);
                    if (expectedCell.Value == null && resultCell.Value == null)
                    {
                        continue;
                    }

                    Assert.True(expectedCell.Value?.Equals(resultCell.Value), $"Cell doesn't match: {expectedCell.Address}");
                }
            }
        }
    }
}