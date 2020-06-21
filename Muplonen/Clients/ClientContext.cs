using Muplonen.DataAccess;

namespace Muplonen.Clients
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public class ClientContext : IClientContext
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
        /// Creates a new <see cref="ClientContext"/> instance.
        /// </summary>
        /// <param name="connection">The connection to the Godot client.</param>
        public ClientContext(GodotClientConnection connection)
        {
            Connection = connection;
        }
    }
}
