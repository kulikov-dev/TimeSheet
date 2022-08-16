using Moq;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;
using TimeSheet.Domain.Providers;

namespace TimeSheet.Tests
{
    /// <summary>
    /// Tests for weekends providers
    /// </summary>
    public class WeekendsProviderTests
    {
        /// <summary>
        /// Report date
        /// </summary>
        private readonly DateTime _reportDate = new(2021, 01, 01);

        /// <summary>
        /// List of expected weekends for January 2021
        /// </summary>
        private readonly List<int> _expectedJanWeeekends = new(30) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 16, 17, 23, 24, 30, 31 };

        /// <summary>
        /// Mock weekends provider
        /// </summary>
        private readonly IWeekendsProvider _mockProvider;

        /// <summary>
        /// Setup
        /// </summary>
        public WeekendsProviderTests()
        {
            var moq = new Mock<IWeekendsProvider>();
            moq.Setup(item => item.GetAnnualWeekends(_reportDate)).Returns(() =>
            {
                var result = Task.FromResult(new AnnualWeekendsInfo(_reportDate.Year));
                result.Result.AddMonthWeekends(1, _expectedJanWeeekends);
                return result;
            });

            _mockProvider = moq.Object;
        }

        [Fact]
        public async void TestMock()
        {
            Assert.NotNull(_mockProvider);

            var annualWeekends = await _mockProvider.GetAnnualWeekends(_reportDate);
            Assert.NotNull(annualWeekends);

            Assert.Equal(_reportDate.Year, annualWeekends.Year);

            var resultJanWeekends = annualWeekends.GetMonthWeekends(_reportDate);
            Assert.True(resultJanWeekends.SequenceEqual(_expectedJanWeeekends));

            Assert.True(annualWeekends.GetMonthWeekends(new DateTime(2021, 02, 01)).Count == 0);
        }

        [Fact]
        public async void TestConsultantProvider()
        {
            var consultantResult = await new ConsultantWeekendsProvider().GetAnnualWeekends(_reportDate);
            var consultantJanWeekends = consultantResult.GetMonthWeekends(_reportDate);

            var expected = _mockProvider.GetAnnualWeekends(_reportDate);
            var expectedJanWeekends = expected.Result.GetMonthWeekends(_reportDate);

            Assert.True(expectedJanWeekends.SequenceEqual(consultantJanWeekends));
        }
    }
}