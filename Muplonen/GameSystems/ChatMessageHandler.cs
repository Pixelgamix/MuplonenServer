using Microsoft.Extensions.Logging;
using Muplonen.SessionManagement;
using System.Threading.Tasks;

namespace Muplonen.GameSystems
{
    /// <summary>
    /// Handles chat messages.
    /// </summary>
    [MessageHandler(IncomingMessages.Chat)]
    public class ChatMessageHandler : IMessageHandler
    {
        private readonly ILogger<ChatMessageHandler> _logger;

        /// <summary>
        /// Creates a new <see cref="ChatMessageHandler"/> instance.
        /// </summary>
        /// <param name="logger">Logging.</param>
        public ChatMessageHandler(
            ILogger<ChatMessageHandler> logger)
        {
            _logger = logger;
        }


        /// <inheritdoc/>
        public async Task<bool> HandleMessage(IPlayerSession session, GodotMessage message)
        {
            if (session.PlayerAccount == null || session.PlayerCharacter == null) return false;

            // Cannot chat when not in a room, but no reason to disconnect...
            if (session.RoomInstance == null) return true;

            var text = message.ReadString();
            _logger.LogInformation("\"{0}\" ({1}) said: \"{2}\"", session.PlayerCharacter.Charactername, session.SessionId, text);

            await session.Connection.Build(OutgoingMessages.Chat, async msg =>
            {
                msg.WriteString(session.PlayerCharacter.Charactername);
                msg.WriteString(text);
                foreach (var recipient in session.RoomInstance.Sessions.AllSessions)
                    await recipient.Connection.Send(msg);
            });

            return true;
        }
    }
}
