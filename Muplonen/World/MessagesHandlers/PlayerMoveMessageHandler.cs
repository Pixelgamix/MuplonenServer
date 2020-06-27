using Muplonen.GameSystems;
using Muplonen.SessionManagement;
using System.Linq;
using System.Threading.Tasks;

namespace Muplonen.World.MessagesHandlers
{
    /// <summary>
    /// Handles player movement requests.
    /// </summary>
    [MessageHandler(IncomingMessages.PlayerMove)]
    public sealed class PlayerMoveMessageHandler : IMessageHandler
    {
        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            if (session.PlayerAccount == null || session.PlayerCharacter == null) return false;

            if (session.RoomInstance == null) return true;

            var position = message.ReadVector3i();
            await session.Connection.Build(OutgoingMessages.OtherPlayerMove, async msg =>
            {
                msg.WriteString(session.PlayerCharacter.Charactername);
                msg.WriteVector3i(position);
                foreach (var recipient in session.RoomInstance.Sessions.AllSessions.Where(other => other != session))
                    await recipient.Connection.Send(msg);
            });

            return true;
        }
    }
}
