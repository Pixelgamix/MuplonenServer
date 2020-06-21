using Muplonen.DataAccess;

namespace Muplonen.Clients
{
    /// <summary>
    /// A player session context.
    /// </summary>
    public interface IClientContext
    {
        /// <summary>
        /// Connection to the Godot client.
        /// </summary>
        GodotClientConnection Connection { get; }

        /// <summary>
        /// The player's account.
        /// </summary>
        PlayerAccount? PlayerAccount { get; set; }
    }
}
