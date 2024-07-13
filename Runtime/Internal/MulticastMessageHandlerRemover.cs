using System;

namespace Packages.UMediator.Runtime.Internal
{
    internal sealed class MulticastMessageHandlerRemover : IDelegateRemover
    {
        private Type _type; 
        private Action<IMulticastMessage> _handler;
        private MulticastMessageHandlerCache _handlers;

        public MulticastMessageHandlerRemover(
            Type type, 
            Action<IMulticastMessage> handler, 
            MulticastMessageHandlerCache handlers)
        {
            _type = type;
            _handler = handler;
            _handlers = handlers;
        }

        public void RemoveHandler()
        {
            _handlers.Remove(_type, _handler);
        }
    }
}