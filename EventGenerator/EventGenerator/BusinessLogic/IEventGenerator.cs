using System.Threading.Tasks;

namespace EventGenerator.BusinessLogic
{
    public interface IEventGenerator
    {
        Task Generate(int count);
    }
}