namespace TimeSheet.Application.Interfaces
{
    public interface ILogWrapper
    {
        public void Error(string message, Exception? ex = null);
        void Info(string message);
        void Warning(string message);
    }
}
