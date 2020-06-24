using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muplonen.Clients
{
    /// <summary>
    /// Accepts <see cref="WebSocket"/> connections and manages a player's session main loop.
    /// </summary>
    public sealed class MuplonenSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<MuplonenSessionMiddleware> _logger;
        private readonly IPlayerSessionManager _clientManager;
        private readonly ObjectPool<GodotMessage> _messageObjectPool;

        /// <summary>
        /// Creates a new <see cref="MuplonenSessionMiddleware"/> instance.
        /// </summary>
        /// <param name="next">The next middleware to execute in the pipeline.</param>
        /// <param name="logger">Logging.</param>
        public MuplonenSessionMiddleware(
            RequestDelegate next,
            ILogger<MuplonenSessionMiddleware> logger,
            IPlayerSessionManager clientManager,
            ObjectPool<GodotMessage> messageObjectPool)
        {
            _next = next;
            _logger = logger;
            _clientManager = clientManager;
            _messageObjectPool = messageObjectPool;
        }

        /// <summary>
        /// Checks, if the request is an incoming WebSocket connection and starts a new player session if that's the case.
        /// </summary>
        /// <param name="context">The request's context.</param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    using var connection = new GodotClientConnection(webSocket, _messageObjectPool);
                    using var clientContext = new PlayerSession(connection);
                    await _clientManager.HandleClient(clientContext);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
