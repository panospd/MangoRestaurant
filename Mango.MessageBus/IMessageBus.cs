using System.Threading.Tasks;

namespace Mango.MessageBus
{
    public interface IMessageBus
    {
        Task PublishMessageAsync(BaseMessage message, string topicName); 
    }
}
