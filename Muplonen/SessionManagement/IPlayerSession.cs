using Muplonen.DataAccess;
using Muplonen.Math;
using Muplonen.World;
using System;
using System.ComponentModel;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public interface IPlayerSession : INotifyPropertyChanged, INotifyPropertyChanging, IDisposable
    {
        /// <summary>
        /// The session's id.
        /// </summary>
        Guid SessionId { get; }

        /// <summary>
        /// Connection to the Godot client.
        /// </summary>
        GodotClientConnection Connection { get; }

        /// <summary>
        /// The player's account.
        /// </summary>
        PlayerAccount? PlayerAccount { get; set; }

        /// <summary>
        /// The player's selected character.
        /// </summary>
        PlayerCharacter? PlayerCharacter { get; set; }

        /// <summary>
        /// The room instance the player is in.
        /// </summary>
        IRoomInstance? RoomInstance { get; set; }

        /// <summary>
        /// The player's position in the current room.
        /// </summary>
        Vector3i Position { get; set; }

        /// <summary>
        /// Event that is triggered when the session ended.
        /// </summary>
        event EventHandler SessionEnded;
    }
}
