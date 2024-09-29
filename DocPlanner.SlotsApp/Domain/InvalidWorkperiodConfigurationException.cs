
namespace DocPlanner.SlotsApp.Domain
{
    public class InvalidWorkperiodConfigurationException : Exception
    {
        public InvalidWorkperiodConfigurationException()
        {
        }

        public InvalidWorkperiodConfigurationException(string msg): base(msg)
        {
        }
    }
}