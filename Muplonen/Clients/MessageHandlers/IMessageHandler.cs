using System.Threading.Tasks;

namespace Muplonen.Clients.MessageHandlers
{
    /// <summary>
    /// Interface for handlers that handle client messages.
    /// </summary>
    public interface IMessageHandler
    {
        /// <summary>
        /// Handles the given message from the specified player session.
        /// </summary>
        /// <param name="session">The player session the message originated from.</param>
        /// <param name="message">The message the client sent.</param>
        /// <returns>true if the message was handled. false, if something was wrong and the session needs to be closed.</returns>
        Task<bool> HandleMessage(IPlayerSession session, GodotMessage message);
    }
}
