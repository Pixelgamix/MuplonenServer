using Muplonen.DataAccess;
using System;

namespace Muplonen.Clients
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public class PlayerSession : IPlayerSession
    {
        /// <inheritdoc/>
        public GodotClientConnection Connection { get; private set; }

        /// <inheritdoc/>
        public PlayerAccount? PlayerAccount { get; set; }

        /// <inheritdoc/>
        public PlayerCharacter? PlayerCharacter { get; set; }

        /// <inheritdoc/>
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
