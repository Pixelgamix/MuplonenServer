using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace Muplonen.Clients
{
    /// <summary>
    /// Accepts <see cref="WebSocket"/> connections and manages a player's session main loop.
    /// </summary>
    public sealed class ClientSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClientSessionMiddleware> _logger;
        private readonly ClientManager _clientManager;

        /// <summary>
        /// Creates a new <see cref="ClientSessionMiddleware"/> instance.
        /// </summary>
        /// <param name="next">The next middleware to execute in the pipeline.</param>
        /// <param name="logger">Logging.</param>
        public ClientSessionMiddleware(
            RequestDelegate next,
            ILogger<ClientSessionMiddleware> logger,
            ClientManager clientManager)
        {
            _next = next;
            _logger = logger;
            _clientManager = clientManager;
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
                    using var connection = new GodotClientConnection(webSocket);
                    await _clientManager.HandleClient(new ClientContext(connection));
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
