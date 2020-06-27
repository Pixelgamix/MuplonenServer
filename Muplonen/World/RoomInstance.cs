using Microsoft.Extensions.Logging;
using Muplonen.DataAccess;
using Muplonen.GameSystems;
using Muplonen.Math;
using Muplonen.SessionManagement;
using System;
using System.Linq;
using System.Numerics;

namespace Muplonen.World
{
    /// <summary>
    /// A room that contains players, NPCs, world information etc.
    /// </summary>
    public sealed class RoomInstance : IRoomInstance
    {
        /// <inheritdoc/>
        public Guid InstanceId { get; } = Guid.NewGuid();

        /// <inheritdoc/>
        public RoomTemplate? Template { get; }

        /// <inheritdoc/>
        public SessionDictionary Sessions { get; } = new SessionDictionary();

        private readonly ILogger<RoomInstance> _logger;

        /// <summary>
        /// Creates a new <see cref="RoomInstance"/>.
        /// </summary>
        public RoomInstance(
            ILogger<RoomInstance> logger)
        {
            Sessions.SessionAdded += Sessions_SessionAdded;
            Sessions.SessionRemoved += Sessions_SessionRemoved;
            _logger = logger;
        }

        /// <summary>
        /// Sends a message to all sessions in the room that the player left.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sessions_SessionRemoved(object? sender, IPlayerSession e)
        {
            _logger.LogInformation("Session ({0}) left room instance ({1})", e.SessionId, InstanceId);

            var firstSession = Sessions.AllSessions.FirstOrDefault();
            if (firstSession == null)
                return;

            _ = firstSession.Connection.Build(OutgoingMessages.OtherPlayerLeftRoom, async msg =>
            {
                msg.WriteString(e.PlayerCharacter!.Charactername);
                foreach (var session in Sessions.AllSessions)
                    await session.Connection.Send(msg);
            });
        }

        /// <summary>
        /// Sends the player that joined the room instance data and sends a message to all
        /// other sessions in the room that the player joined.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Sessions_SessionAdded(object? sender, IPlayerSession e)
        {
            _logger.LogInformation("Session ({0}) joined room instance ({1})", e.SessionId, InstanceId);

            // Send room information to session that was added
            _ = e.Connection.BuildAndSend(OutgoingMessages.SelfEnterRoom, msg =>
            {
                var othersInRoom = Sessions.AllSessions.Where(other => other != e).ToArray();
                msg.WriteUInt16((ushort)othersInRoom.Length);
                foreach (var other in othersInRoom)
                {
                    msg.WriteString(other.PlayerCharacter!.Charactername);
                    msg.WriteVector3i(other.Position);
                }
            });

            // Inform others about the new session
            _ = e.Connection.Build(OutgoingMessages.OtherPlayerEnterRoom, async msg =>
            {
                msg.WriteString(e.PlayerCharacter!.Charactername);
                msg.WriteVector3i(Vector3i.Zero);
                foreach (var session in Sessions.AllSessions.Where(recipient => recipient != e))
                    await session.Connection.Send(msg);
            });
        }
    }
}
