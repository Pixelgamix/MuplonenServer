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
    public sealed class ClientManager
    {
        /// <summary>
        /// Bag with clients that did not authenticate themselves yet.
        /// </summary>
        public ConcurrentBag<GodotClientConnection> ClientsWithoutAuthentication { get; } = new ConcurrentBag<GodotClientConnection>();

        /// <summary>
        /// Dictionary holding all authenticated and active clients.
        /// </summary>
        public ConcurrentDictionary<Guid, PlayerSession> Clients { get; } = new ConcurrentDictionary<Guid, PlayerSession>();

        private readonly MessageHandlerTypes _messageHandlerTypes;
        private readonly ObjectPool<GodotMessage> _messageObjectPool;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ClientManager> _logger;

        /// <summary>
        /// Creates a new <see cref="ClientManager"/> instance.
        /// </summary>
        /// <param name="messageHandlerTypes">Available message handler types.</param>
        /// <param name="messageObjectPool">Pool for message objects.</param>
        /// <param name="serviceProvider">Service provider for fetching implementations.</param>
        /// <param name="logger">Logging.</param>
        public ClientManager(
            MessageHandlerTypes messageHandlerTypes,
            ObjectPool<GodotMessage> messageObjectPool,
            IServiceProvider serviceProvider,
            ILogger<ClientManager> logger)
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
                var isMessageReceived = await playerSession.Connection.Read(godotMessage);
                while (isMessageReceived)
                {
                    // Read message id and fetch message handler for the id
                    ushort messageId = godotMessage.ReadUInt16();

                    if (_messageHandlerTypes.TryGetHandlerForMessageId(scopedServiceProvider.ServiceProvider, messageId, out IMessageHandler? messageHandler))
                    {
                        await messageHandler!.HandleMessage(playerSession, godotMessage);
                        isMessageReceived = await playerSession.Connection.Read(godotMessage);
                    }
                    else
                    {
                        _logger.LogDebug("Received unknown message id {0} from session {1}. Aborting session loop.", messageId, playerSession.SessionId);
                        isMessageReceived = false;
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
                await playerSession.Connection.Close();
                _messageObjectPool.Return(godotMessage);
            }
        }
    }
}
