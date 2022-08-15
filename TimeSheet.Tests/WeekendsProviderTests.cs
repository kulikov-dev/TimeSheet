using Moq;
using TimeSheet.Application.Interfaces;
using TimeSheet.Data;
using TimeSheet.Domain.Providers;

namespace TimeSheet.Tests
{
    public class WeekendsProviderTests
    {
        private readonly DateTime _date = new(2021, 01, 01);
        private readonly List<int> _weekends = new(30) { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 16, 17, 23, 24, 30, 31 };
        private readonly IWeekendsProvider _weekendsProvider;

        public WeekendsProviderTests()
        {
            var moq = new Mock<IWeekendsProvider>();
            moq.Setup(item => item.GetAnnualWeekends(_date)).Returns(() =>
            {
                var result = Task.FromResult(new AnnualWeekendsInfo(_date.Year));
                result.Result.AddMonthWeekends(1, _weekends);
                return result;
            });

            _weekendsProvider = moq.Object;
        }

        [Fact]
        public async void TestMock()
        {
            Assert.NotNull(_weekendsProvider);
            var annualWeekends = await _weekendsProvider.GetAnnualWeekends(_date);
            Assert.NotNull(annualWeekends);
            Assert.Equal(_date.Year, annualWeekends.Year);

            var janWeekends = annualWeekends.GetMonthWeekends(_date);
            Assert.True(janWeekends.SequenceEqual(_weekends));
        }

        [Fact]
        public async void TestConsultantProvider()
        {
            var consultantResult = await new ConsultantWeekendsProvider().GetAnnualWeekends(_date);
            var consultantJanWeekends = consultantResult.GetMonthWeekends(_date);

            var expected = _weekendsProvider.GetAnnualWeekends(_date);
            var expectedJanWeekends = expected.Result.GetMonthWeekends(_date);

            Assert.True(expectedJanWeekends.SequenceEqual(consultantJanWeekends));
        }
    }
}