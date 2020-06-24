using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muplonen.Clients.MessageHandlers
{
    /// <summary>
    /// Provides access to all known message handlers.
    /// </summary>
    public class MessageHandlerTypes
    {
        private readonly Dictionary<ushort, Type> _messageHandlerTypes;

        /// <summary>
        /// Creates a new <see cref="MessageHandlerTypes"/> instance initialized with all
        /// message handlers available.
        /// </summary>
        public MessageHandlerTypes()
        {
            _messageHandlerTypes = FindMessageHandlersInAssembly()
                .ToDictionary(type => type.GetCustomAttribute<MessageHandlerAttribute>()!.MessageId);
        }

        /// <summary>
        /// Scans the assembly for message handlers.
        /// </summary>
        /// <returns>Enumeration of available message handlers.</returns>
        public static IEnumerable<Type> FindMessageHandlersInAssembly()
        {
            return typeof(MessageHandlerTypes).Assembly.GetTypes()
                .Where(type => typeof(IMessageHandler).IsAssignableFrom(type) && type.GetCustomAttribute<MessageHandlerAttribute>() != null);
        }

        /// <summary>
        /// Fetches and returns the <see cref="IMessageHandler"/> that is able to handle the message
        /// with the specified messageId.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> to create the message handler with.</param>
        /// <param name="messageId">The message id that needs to be handled.</param>
        /// <param name="messageHandler">The handler able to handle the message.</param>
        /// <returns>true, if a handler for the message id was returned. Otherwise false.</returns>
        public bool TryGetHandlerForMessageId(IServiceProvider serviceProvider, ushort messageId, out IMessageHandler? messageHandler)
        {
            if (!_messageHandlerTypes.TryGetValue(messageId, out Type? messageHandlerType))
            {
                messageHandler = null;
                return false;
            }

            messageHandler = (IMessageHandler)serviceProvider.GetRequiredService(messageHandlerType);
            return true;
        }
    }
}
