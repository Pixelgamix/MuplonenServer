using System.Threading.Tasks;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// Interfaces for managers that manage player sessions.
    /// </summary>
    public interface IPlayerSessionManager
    {
        /// <summary>
        /// Dictionary holding all sessions.
        /// </summary>
        SessionDictionary Sessions { get; }

        /// <summary>
        /// The player session's main loop. Handles incoming messages.
        /// </summary>
        /// <param name="playerSession">The players's session.</param>
        /// <returns></returns>
        Task HandleClient(PlayerSession playerSession);
    }
}
