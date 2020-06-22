using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace Muplonen.Clients
{
    /// <summary>
    /// Connection to a Godot client.
    /// </summary>
    public sealed class GodotClientConnection : IDisposable
    {
        /// <summary>
        /// Status information from the last call to <see cref="Read(GodotMessage)"/>.
        /// </summary>
        public WebSocketReceiveResult Status { get; private set; } = new WebSocketReceiveResult(0, WebSocketMessageType.Binary, true);

        private readonly WebSocket _webSocket;
        private bool _isClosed;

        /// <summary>
        /// Creates a new <see cref="GodotClientConnection"/> instance that uses the specified
        /// <see cref="WebSocket"/> to communicate with the client.
        /// </summary>
        /// <param name="webSocket">The WebSocket to use.</param>
        public GodotClientConnection(WebSocket webSocket)
        {
            _webSocket = webSocket;
        }

        /// <summary>
        /// Reads a message sent by the client.
        /// </summary>
        /// <param name="message">The message that was read.</param>
        /// <returns>true if a message was received, otherwise false.</returns>
        public async Task<bool> Read(GodotMessage message)
        {
            if (_isClosed)
                return false;

            message.ResetPosition();
            Status = await _webSocket.ReceiveAsync(new ArraySegment<byte>(message.Buffer, 0, message.Buffer.Length), CancellationToken.None);
            return !Status.CloseStatus.HasValue;
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task Send(GodotMessage message)
        {
            if (_isClosed)
                return Task.CompletedTask;

            return _webSocket.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.WritePosition), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// Closes the connection to the client. Uses the status information from <see cref="Status"/>, if its CloseStatus
        /// has a value. Otherwise the specified CloseStatus and StatusDescription are used.
        /// </summary>
        /// <param name="closeStatus">Reason for closing the connection (if no Status.CloseStatus is set).</param>
        /// <param name="statusDescription">Description of the reason (if no Status.CloseStatus is set).</param>
        /// <returns></returns>
        public Task Close(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.Empty, string statusDescription = "")
        {
            if (_isClosed)
                return Task.CompletedTask;

            _isClosed = true;

            // WebSocket must be in state "Open", "CloseReceived" or "CloseSent"
            if (_webSocket.State != WebSocketState.Open && _webSocket.State != WebSocketState.CloseReceived && _webSocket.State != WebSocketState.CloseSent)
                return Task.CompletedTask;

            if (Status.CloseStatus.HasValue)
                return _webSocket.CloseAsync(Status.CloseStatus.Value, Status.CloseStatusDescription, CancellationToken.None);

            return _webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Frees ressources used by the connection.
        /// </summary>
        public void Dispose()
        {
            Close();
            _webSocket.Dispose();
        }
    }
}
