using Muplonen.DataAccess;
using Muplonen.SessionManagement;
using System;

namespace Muplonen.World
{
    /// <summary>
    /// A room that contains players, NPCs, world information etc.
    /// </summary>
    public interface IRoomInstance
    {
        /// <summary>
        /// The room instance's id.
        /// </summary>
        Guid InstanceId { get; }

        /// <summary>
        /// The template the room was created from.
        /// </summary>
        RoomTemplate? Template { get; }

        /// <summary>
        /// Player sessions in this room.
        /// </summary>
        SessionDictionary Sessions { get; }
    }
}
