using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Muplonen.Clients.Messages;
using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muplonen.Clients
{
    /// <summary>
    /// Manages game clients.
    /// </summary>
    public sealed class PlayerSessionManager
    {
        /// <summary>
        /// Dictionary holding all authenticated and active clients.
        /// </summary>
        public ConcurrentDictionary<Guid, IPlayerSession> Clients { get; } = new ConcurrentDictionary<Guid, IPlayerSession>();

        private readonly MessageHandlerTypes _messageHandlerTypes;
        private readonly ObjectPool<GodotMessage> _messageObjectPool;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PlayerSessionManager> _logger;

        /// <summary>
        /// Creates a new <see cref="PlayerSessionManager"/> instance.
        /// </summary>
        /// <param name="messageHandlerTypes">Available message handler types.</param>
        /// <param name="messageObjectPool">Pool for message objects.</param>
        /// <param name="serviceProvider">Service provider for fetching implementations.</param>
        /// <param name="logger">Logging.</param>
        public PlayerSessionManager(
            MessageHandlerTypes messageHandlerTypes,
            ObjectPool<GodotMessage> messageObjectPool,
            IServiceProvider serviceProvider,
            ILogger<PlayerSessionManager> logger)
        {
            _messageHandlerTypes = messageHandlerTypes;
            _messageObjectPool = messageObjectPool;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// The player session's main loop. Handles incoming messages.
        /// </summary>
        /// <param name="playerSession">The players's session.</param>
        /// <returns></returns>
        public async Task HandleClient(PlayerSession playerSession)
        {
            var godotMessage = _messageObjectPool.Get();

            using var scopedServiceProvider = _serviceProvider.CreateScope();

            try
            {
                var isSessionAlive = await playerSession.Connection.Read(godotMessage);
                while (isSessionAlive)
                {
                    // Read message id and fetch message handler for the id
                    ushort messageId = godotMessage.ReadUInt16();

                    if (_messageHandlerTypes.TryGetHandlerForMessageId(scopedServiceProvider.ServiceProvider, messageId, out IMessageHandler? messageHandler))
                    {
                        isSessionAlive = await messageHandler!.HandleMessage(playerSession, godotMessage);
                        if (isSessionAlive)
                            isSessionAlive = await playerSession.Connection.Read(godotMessage);
                    }
                    else
                    {
                        _logger.LogDebug("Received unknown message id {0} from session {1}. Aborting session loop.", messageId, playerSession.SessionId);
                        isSessionAlive = false;
                    }
                }
            }
            catch (WebSocketException ex)
            {
                // WebSocketExceptions are usually triggered by a client sending trash data and/or forcefully closing the connection.
                _logger.LogDebug("Communication error with session {0}: {1}", playerSession.SessionId, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while handling session {0}.", playerSession.SessionId);
            }
            finally
            {
                if (playerSession.PlayerAccount != null)
                    Clients.TryRemove(playerSession.PlayerAccount.Id, out _);

                _logger.LogInformation("Disconnecting session {0}", playerSession.SessionId);

                await playerSession.Connection.Close();
                _messageObjectPool.Return(godotMessage);
            }
        }
    }
}
