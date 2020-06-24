using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Muplonen.Clients
{
    /// <summary>
    /// Interfaces for managers that manage player sessions.
    /// </summary>
    public interface IPlayerSessionManager
    {
        /// <summary>
        /// Dictionary holding all authenticated and active clients.
        /// </summary>
        ConcurrentDictionary<Guid, IPlayerSession> Clients { get; }

        /// <summary>
        /// The player session's main loop. Handles incoming messages.
        /// </summary>
        /// <param name="playerSession">The players's session.</param>
        /// <returns></returns>
        Task HandleClient(PlayerSession playerSession);
    }
}
