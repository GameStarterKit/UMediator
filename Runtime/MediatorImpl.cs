using System;
using System.Reflection;
using Packages.UMediator.Runtime.Internal;

namespace Packages.UMediator.Runtime 
{
    public sealed class MediatorImpl : IMediator
    {
        private SingleMessageHandlerCache _singleMessageHandlers = new SingleMessageHandlerCache();
        
        private MulticastMessageHandlerCache _multicastMessageHandlers = new MulticastMessageHandlerCache();
        
        private ActiveObjectTracker _activeObjects = new ActiveObjectTracker();
        
        private static MediatorImpl _instance;

        public void Publish(IMulticastMessage message)
        {
            _multicastMessageHandlers.Invoke(message);
        }

        public T Send<T>(ISingleMessage<T> message)
        {
            return _singleMessageHandlers.Invoke(message);
        }
        
        private void CacheMulticastMessageHandler(Type messageType, object instance, MethodInfo method)
        {
            var handler = _multicastMessageHandlers.CacheHandler(messageType, instance, method);
            var remover = new MulticastMessageHandlerRemover(messageType, handler, _multicastMessageHandlers);
        }

        private void CacheSingletMessageHandler(Type messageType, object instance, MethodInfo method)
        {
            var returnType = messageType.GetInterfaces()[0].GenericTypeArguments[0];
            _singleMessageHandlers.CacheHandler(messageType, returnType, instance, method);
            var remover = new SingleMessageHandlerRemover(messageType, _singleMessageHandlers);
        }
    }
}
        