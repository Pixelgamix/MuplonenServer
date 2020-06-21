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
        public ConcurrentDictionary<Guid, ClientContext> Clients { get; } = new ConcurrentDictionary<Guid, ClientContext>();

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
        /// <param name="clientContext">The client's context.</param>
        /// <returns></returns>
        public async Task HandleClient(ClientContext clientContext)
        {
            var godotMessage = _messageObjectPool.Get();

            using var scopedServiceProvider = _serviceProvider.CreateScope();

            try
            {

                var result = await clientContext.Connection.Read(ref godotMessage);
                while (!result.CloseStatus.HasValue)
                {
                    godotMessage.ResetPosition();

                    // Read message id and fetch message handler for the id
                    ushort messageId = godotMessage.ReadUInt16();
                    var messageHandler = _messageHandlerTypes.GetHandlerForMessageId(scopedServiceProvider.ServiceProvider, messageId);

                    if (messageHandler != null)
                    {
                        await messageHandler.HandleMessage(clientContext, godotMessage);
                        result = await clientContext.Connection.Read(ref godotMessage);
                    }
                    else
                    {
                        var errorMessage = $"Unknown message id: { messageId}";
                        _logger.LogDebug(errorMessage);
                        await clientContext.Connection.Close(WebSocketCloseStatus.ProtocolError, errorMessage);
                        break;
                    }
                }
                if (result.CloseStatus.HasValue)
                    await clientContext.Connection.Close(result.CloseStatus.Value, result.CloseStatusDescription);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during session loop. Closing connection to client.");
                await clientContext.Connection.Close(WebSocketCloseStatus.InternalServerError, "Internal Server Error");
            }
            finally
            {
                _messageObjectPool.Return(godotMessage);
            }
        }
    }
}
