using Microsoft.Extensions.ObjectPool;
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

        /// <summary>
        /// Timestamp of the last message received by the client.
        /// </summary>
        public DateTime LastMessageReceivedAt { get; private set; }

        private readonly WebSocket _webSocket;
        private readonly ObjectPool<GodotMessage> _messageObjectPool;
        private bool _isClosed;

        /// <summary>
        /// Creates a new <see cref="GodotClientConnection"/> instance that uses the specified
        /// <see cref="WebSocket"/> to communicate with the client.
        /// </summary>
        /// <param name="webSocket">The WebSocket to use.</param>
        /// <param name="messageObjectPool">Pool for creating messages.</param>
        public GodotClientConnection(
            WebSocket webSocket,
            ObjectPool<GodotMessage> messageObjectPool)
        {
            _webSocket = webSocket;
            _messageObjectPool = messageObjectPool;
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
            if (Status.CloseStatus.HasValue)
                return false;

            LastMessageReceivedAt = DateTime.Now;
            return true;
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
        /// Allows the caller to build a message that will be send to the client afterwards.
        /// </summary>
        /// <param name="messageId">The message's id.</param>
        /// <param name="messageBuilder">The action to populate the message with data.</param>
        /// <returns></returns>
        public async Task BuildAndSend(ushort messageId, Action<GodotMessage> messageBuilder)
        {
            var message = _messageObjectPool.Get();
            try
            {
                message.WriteUInt16(messageId);
                messageBuilder(message);
                await _webSocket.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.WritePosition), WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            finally
            {
                _messageObjectPool.Return(message);
            }
        }

        /// <summary>
        /// Closes the connection to the client. Uses the status information from <see cref="Status"/>, if its CloseStatus
        /// has a value. Otherwise the specified CloseStatus and StatusDescription are used.
        /// </summary>
        /// <param name="closeStatus">Reason for closing the connection (if no Status.CloseStatus is set).</param>
        /// <param name="statusDescription">Description of the reason (if no Status.CloseStatus is set).</param>
        /// <returns></returns>
        public async Task Close(WebSocketCloseStatus closeStatus = WebSocketCloseStatus.Empty, string statusDescription = "")
        {
            if (_isClosed)
                return;

            _isClosed = true;

            // WebSocket must be in state "Open", "CloseReceived" or "CloseSent"
            if (_webSocket.State != WebSocketState.Open && _webSocket.State != WebSocketState.CloseReceived && _webSocket.State != WebSocketState.CloseSent)
                return;

            if (Status.CloseStatus.HasValue)
                await _webSocket.CloseAsync(Status.CloseStatus.Value, Status.CloseStatusDescription, CancellationToken.None);
            else
                await _webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
        }

        /// <summary>
        /// Frees ressources used by the connection.
        /// </summary>
        public void Dispose()
        {
            _webSocket.Dispose();
        }
    }
}
