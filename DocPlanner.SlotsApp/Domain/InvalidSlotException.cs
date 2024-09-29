
namespace DocPlanner.SlotsApp.Domain
{
    public class InvalidSlotException : Exception
    {
        public InvalidSlotException(string message) : base(message)
        {
        }
    }
}