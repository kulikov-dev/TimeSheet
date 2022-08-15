using Moq;
using TimeSheet.Application.Interfaces;
using TimeSheet.Application.Logger;

namespace TimeSheet.Tests
{
    public class LogTests
    {
        [Fact]
        public void TestILogWrapper()
        {
            var mock = new Mock<ILogWrapper>();
            Log.Attach(mock.Object);

            Log.Info("Info message");

            mock.Verify(objectItem => objectItem.Info(It.IsAny<string>()));
            mock.Verify(objectItem => objectItem.Info("Info message"));
            mock.Verify(objectItem => objectItem.Info(It.IsAny<string>()), Times.Once());
        }
    }
}