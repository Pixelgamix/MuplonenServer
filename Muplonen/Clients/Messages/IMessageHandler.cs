using System.Threading.Tasks;

namespace Muplonen.Clients.Messages
{
    public interface IMessageHandler
    {
        Task HandleMessage(IClientContext session, GodotMessage message);
    }
}
