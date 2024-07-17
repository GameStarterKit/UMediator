using System;
using System.Collections.Generic;
using System.Reflection;
using Packages.UMediator.Runtime.Internal;
using UnityEngine;

namespace Packages.UMediator.Runtime 
{
    public sealed class MediatorImpl : IMediator
    {
        private readonly SingleMessageHandlerCache _singleMessageHandlers = new();
        private readonly MulticastMessageHandlerCache _multicastMessageHandlers = new();
        
        private static MediatorImpl _instance;

        private List<HandlerToMessageData> GetAllHandlersInRegisteredAssemblies(IEnumerable<Assembly> assemblies)
        {
            var messageData = new List<HandlerToMessageData>();
            
            foreach (var assembly in assemblies)
            {
                var allTypes = assembly.GetTypes();
                
                foreach (var type in allTypes)
                {
                    if(type.IsAbstract) continue;
                    if(type.IsSubclassOf(typeof(MonoBehaviour))) continue;

                    var interfaces = type.GetCachedInterfaces();
                    foreach (var iInterface in interfaces)
                    {
                        if (!iInterface.IsGenericType)
                        {
                            continue;
                        }
                        var genericType = iInterface.GetGenericTypeDefinition();

                        HandlerToMessageData handlerToMessageData = default;

                        if (typeof(IMulticastMessageHandler<>) != genericType &&
                            typeof(ISingleMessageHandler<>) != genericType &&
                            typeof(ISingleMessageHandler<,>) != genericType)
                        {
                            continue;
                        }
                        handlerToMessageData = GetHandlerData(type, iInterface.GenericTypeArguments);
                        messageData.Add(handlerToMessageData);
                    }
                }
            }


            return messageData;
        }

        private static HandlerToMessageData GetHandlerData(Type type, Type[] typeArguments)
        {
            var methods = type.GetCachedMethods();
            for (int i = 0; i < methods.Length; i++)
            {
                if(!methods[i].Name.Equals("Handle", StringComparison.InvariantCulture))
                {
                    continue;
                }

                var messageType = typeArguments[0];
                HandlerToMessageData data = new HandlerToMessageData()
                {
                    HandlerType = type,
                    MessageType = messageType,
                    ReturnType = methods[i].ReturnType,
                    Method = methods[i]
                };
                return data;
            }

            throw new Exception("Something went wrong");
        }

        private struct HandlerToMessageData
        {
            public Type HandlerType;
            public Type ReturnType;

            public Type MessageType;
            public MethodInfo Method;
        }

        public MediatorImpl(IEnumerable<Assembly> assemblies)
        {
            var types = GetAllHandlersInRegisteredAssemblies(assemblies);
            foreach (var data in types)
            {
                var handlerObject = Activator.CreateInstance(data.HandlerType);
                if (typeof(IMulticastMessage).IsAssignableFrom(data.MessageType))
                {
                    CacheMulticastMessageHandler(data.MessageType, handlerObject, data.Method);
                }
                else if (typeof(ISingleMessage).IsAssignableFrom(data.MessageType))
                {
                    CacheSingletMessageHandler(data.MessageType, data.ReturnType, handlerObject, data.Method);
                }
            }
        }
        public void Publish(IMulticastMessage message)
        {
            _multicastMessageHandlers.Invoke(message);
        }

        public void Send(ISingleMessage message)
        {
            _singleMessageHandlers.Invoke(message);
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

        private void CacheSingletMessageHandler(Type messageType, Type returnType, object instance, MethodInfo method)
        {
            _singleMessageHandlers.CacheHandler(messageType, returnType, instance, method);
            var remover = new SingleMessageHandlerRemover(messageType, _singleMessageHandlers);
        }
    }
}
        