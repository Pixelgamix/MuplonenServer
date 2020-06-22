using Muplonen.DataAccess;
using System;

namespace Muplonen.Clients
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public class PlayerSession : IPlayerSession
    {
        /// <summary>
        /// Connection to the Godot client.
        /// </summary>
        public GodotClientConnection Connection { get; private set; }

        /// <summary>
        /// The player's account.
        /// </summary>
        public PlayerAccount? PlayerAccount { get; set; }

        /// <summary>
        /// The session's id.
        /// </summary>
        public Guid SessionId { get; } = Guid.NewGuid();

        /// <summary>
        /// Creates a new <see cref="PlayerSession"/> instance.
        /// </summary>
        /// <param name="connection">The connection to the Godot client.</param>
        public PlayerSession(GodotClientConnection connection)
        {
            Connection = connection;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}
