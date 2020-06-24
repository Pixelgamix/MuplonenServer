using Muplonen.DataAccess;
using System;

namespace Muplonen.Clients
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public interface IPlayerSession : IDisposable
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
    }
}
