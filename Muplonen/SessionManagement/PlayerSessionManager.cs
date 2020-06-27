using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Muplonen.GameSystems;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muplonen.SessionManagement
{
    /// <summary>
    /// Manages player sessions.
    /// </summary>
    public sealed class PlayerSessionManager : IPlayerSessionManager
    {
        /// <inheritdoc/>
        public SessionDictionary Sessions { get; } = new SessionDictionary();

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

        /// <inheritdoc/>
        public async Task HandleClient(PlayerSession playerSession)
        {
            var godotMessage = _messageObjectPool.Get();

            using var scopedServiceProvider = _serviceProvider.CreateScope();

            Sessions.TryAddSession(playerSession);

            try
            {
                var isSessionAlive = await playerSession.Connection.Read(godotMessage);
                while (isSessionAlive)
                {
                    // Read message id and fetch message handler for the id
                    var messageId = godotMessage.ReadUInt16();

                    if (_messageHandlerTypes.TryGetHandlerForMessageId(scopedServiceProvider.ServiceProvider, messageId, out IMessageHandler? messageHandler))
                    {
                        isSessionAlive = await messageHandler!.HandleMessage(playerSession, godotMessage);
                        if (isSessionAlive)
                            isSessionAlive = await playerSession.Connection.Read(godotMessage);
                    }
                    else
                    {
                        _logger.LogInformation("Received unknown message id {0} from session {1}. Aborting session loop.", messageId, playerSession.SessionId);
                        isSessionAlive = false;
                    }
                }
            }
            catch (WebSocketException ex)
            {
                // WebSocketExceptions are usually triggered by a client sending trash data and/or forcefully closing the connection.
                _logger.LogInformation("Communication error with session {0}: {1}", playerSession.SessionId, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while handling session {0}.", playerSession.SessionId);
            }
            finally
            {
                Sessions.RemoveSession(playerSession);

                _logger.LogInformation("Disconnecting session {0}", playerSession.SessionId);

                await playerSession.Connection.Close();
                _messageObjectPool.Return(godotMessage);
            }
        }
    }
}
