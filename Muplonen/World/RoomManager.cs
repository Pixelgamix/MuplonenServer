using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muplonen.SessionManagement;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Muplonen.World
{
    /// <summary>
    /// Room manager.
    /// </summary>
    public sealed class RoomManager : IRoomManager
    {
        private readonly ConcurrentDictionary<Guid, IRoomInstance> _roomInstances = new ConcurrentDictionary<Guid, IRoomInstance>();
        private readonly ILogger<RoomManager> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Creates a new <see cref="RoomManager"/> instance.
        /// </summary>
        /// <param name="logger">Logging.</param>
        /// <param name="serviceProvider">Service provider for creating <see cref="RoomInstance"/>s.</param>
        public RoomManager(
            ILogger<RoomManager> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public Task AddToRoomInstance(IPlayerSession playerSession, Guid roomInstanceId)
        {
            if (!_roomInstances.TryGetValue(roomInstanceId, out IRoomInstance? roomInstance))
            {
                // FIXME: Replace placeholder with real implementation
                roomInstance = _serviceProvider.GetRequiredService<IRoomInstance>();
                _roomInstances.TryAdd(roomInstanceId, roomInstance);
            }

            if (!roomInstance.Sessions.TryAddSession(playerSession))
            {
                _logger.LogError("Failed to add session ({0}) to room instance ({1}).", playerSession.SessionId, roomInstance.InstanceId);
                return Task.CompletedTask;
            }

            playerSession.SessionEnded -= PlayerSession_SessionEnded;
            playerSession.SessionEnded += PlayerSession_SessionEnded;
            playerSession.RoomInstance = roomInstance;
            roomInstance.Sessions.TryAddSession(playerSession);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes the session that ended from the room it was in and notifies all other people in the room that
        /// the player left.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerSession_SessionEnded(object? sender, EventArgs e)
        {
            if (sender == null) return;

            // Unregister from manager
            var playerSession = (IPlayerSession)sender;
            playerSession.SessionEnded -= PlayerSession_SessionEnded;

            if (playerSession.RoomInstance == null) return;

            // Remove player from room instance
            var roomInstance = playerSession.RoomInstance;
            playerSession.RoomInstance = null;
            roomInstance.Sessions.RemoveSession(playerSession);
        }
    }
}
