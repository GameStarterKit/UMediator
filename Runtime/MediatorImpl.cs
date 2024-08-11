using System;
using System.Collections.Generic;
using System.Reflection;
using Packages.UMediator.Runtime.Internal;
using UnityEngine;

namespace Packages.UMediator.Runtime 
{
    public class MediatorImpl : IMediator
    {
        protected readonly SingleMessageHandlerCache SingleMessageHandlers = new();
        protected readonly MulticastMessageHandlerCache MulticastMessageHandlers = new();
        
        protected HashSet<Assembly> Assemblies;
        protected Action<object> InjectionDelegate;
        protected bool AreHandlersCached;

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

                        if (typeof(IMulticastMessageHandler<>) != genericType &&
                            typeof(ISingleMessageHandler<>) != genericType &&
                            typeof(ISingleMessageHandler<,>) != genericType)
                        {
                            continue;
                        }

                        var handlerToMessageData = GetHandlerData(type, iInterface.GenericTypeArguments);
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

            throw new Exception("Unable to get cached methods from types.");
        }

        protected struct HandlerToMessageData
        {
            public Type HandlerType;
            public Type ReturnType;

            public Type MessageType;
            public MethodInfo Method;
        }

        public virtual void RegisterAssemblies(IEnumerable<Assembly> assemblies)
        {
            Assemblies ??= new HashSet<Assembly>();

            foreach (var assembly in assemblies)
            {
                Assemblies.Add(assembly);
            }
        }

        public virtual void RegisterAssembly(Assembly assembly)
        {
            Assemblies ??= new HashSet<Assembly>();
            Assemblies.Add(assembly);
        }

        public virtual void RegisterDiDelegate(Action<object> injectionDelegate)
        {
            InjectionDelegate = injectionDelegate;
        }

        public virtual void CacheMessageHandlers()
        {
            if (AreHandlersCached)
            {
                return;
            }

            CacheHandlersInAssemblies(Assemblies);
            
            AreHandlersCached = true;
        }

        private void CacheHandlersInAssemblies(IEnumerable<Assembly> assemblies)
        {
            var types = GetAllHandlersInRegisteredAssemblies(assemblies);

            foreach (var data in types)
            {
                var handlerObject = Activator.CreateInstance(data.HandlerType);
                InjectionDelegate?.Invoke(handlerObject);

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

        public virtual void Publish(IMulticastMessage message)
        {
            CacheMessageHandlers();
            MulticastMessageHandlers.Invoke(message);
        }

        public virtual void Send(ISingleMessage message)
        {
            CacheMessageHandlers();
            SingleMessageHandlers.Invoke(message);
        }
        
        public virtual T Send<T>(ISingleMessage<T> message)
        {
            CacheMessageHandlers();
            return SingleMessageHandlers.Invoke(message);
        }
        
        private void CacheMulticastMessageHandler(Type messageType, object instance, MethodInfo method)
        {
            var handler = MulticastMessageHandlers.CacheHandler(messageType, instance, method);
            var remover = new MulticastMessageHandlerRemover(messageType, handler, MulticastMessageHandlers);
        }

        private void CacheSingletMessageHandler(Type messageType, Type returnType, object instance, MethodInfo method)
        {
            SingleMessageHandlers.CacheHandler(messageType, returnType, instance, method);
            var remover = new SingleMessageHandlerRemover(messageType, SingleMessageHandlers);
        }
    }
}
        