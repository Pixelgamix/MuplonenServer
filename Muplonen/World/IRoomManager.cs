using Muplonen.SessionManagement;
using System;
using System.Threading.Tasks;

namespace Muplonen.World
{
    /// <summary>
    /// Room manager.
    /// </summary>
    public interface IRoomManager
    {
        /// <summary>
        /// Adds the specified player session to the specified room instance.
        /// </summary>
        /// <param name="playerSession">The player session.</param>
        /// <param name="roomInstanceId">The room instance.</param>
        /// <returns></returns>
        Task AddToRoomInstance(IPlayerSession playerSession, Guid roomInstanceId);
    }
}
