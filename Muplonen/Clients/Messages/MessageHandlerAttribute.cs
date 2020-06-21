using System;

namespace Muplonen.Clients.Messages
{
    /// <summary>
    /// Attribute for message handlers that defines which message id a handler is used for.
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class MessageHandlerAttribute : Attribute
    {
        /// <summary>
        /// The message id the handler handles.
        /// </summary>
        public ushort MessageId { get; }

        /// <summary>
        /// Specifies the message id the handler handels.
        /// </summary>
        /// <param name="messageId">The message id the handler handles.</param>
        public MessageHandlerAttribute(ushort messageId)
        {
            MessageId = messageId;
        }
    }
}
