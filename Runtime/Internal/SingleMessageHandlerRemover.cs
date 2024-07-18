using System;

namespace Packages.UMediator.Runtime.Internal
{
    internal sealed class SingleMessageHandlerRemover : IDelegateRemover
    {
        private readonly Type _messageType;
        private readonly SingleMessageHandlerCache _handlers;

        public SingleMessageHandlerRemover(
            Type messageType, 
            SingleMessageHandlerCache handlers)
        {
            _messageType = messageType;
            _handlers = handlers;
        }
        
        public void RemoveHandler()
        {
            _handlers.Remove(_messageType);
        }
    }
}