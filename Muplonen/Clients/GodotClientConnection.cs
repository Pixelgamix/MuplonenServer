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
        private readonly WebSocket _webSocket;

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
        /// <returns></returns>
        public Task<WebSocketReceiveResult> Read(ref GodotMessage message)
        {
            return _webSocket.ReceiveAsync(new ArraySegment<byte>(message.Buffer, 0, message.Buffer.Length), CancellationToken.None);
        }

        /// <summary>
        /// Sends a message to the client.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns></returns>
        public Task Send(GodotMessage message)
        {
            return _webSocket.SendAsync(new ArraySegment<byte>(message.Buffer, 0, message.WritePosition), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        /// <summary>
        /// Closes the connection to the client.
        /// </summary>
        /// <param name="closeStatus">Reason for closing the connection.</param>
        /// <param name="statusDescription">Description of the reason.</param>
        /// <returns></returns>
        public Task Close(WebSocketCloseStatus closeStatus, string statusDescription)
        {
            return _webSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
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
