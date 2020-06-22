using System.Threading.Tasks;

namespace Muplonen.Clients.Messages
{
    public interface IMessageHandler
    {
        Task HandleMessage(IPlayerSession session, GodotMessage message);
    }
}
