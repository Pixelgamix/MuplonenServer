using Muplonen.GameSystems;
using System.Threading.Tasks;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// Handles incoming ping messages.
    /// </summary>
    [MessageHandler(IncomingMessages.Ping)]
    public class PingMessageHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            await session.Connection.BuildAndSend(OutgoingMessages.Pong, reply => { });
            return true;
        }
    }
}
