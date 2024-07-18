using System;
using System.Collections.Generic;
using System.Reflection;

namespace Packages.UMediator.Runtime.Internal
{
    public class SingleMessageHandlerCache
    {
        private readonly Dictionary<Type, object> _handlers = new Dictionary<Type, object>();
        
        public void Add<TMessage, TReturn>(Func<TMessage, TReturn> handler)
        {
            if (_handlers.ContainsKey(typeof(TMessage)))
            {
                throw new UniMediatorException(
                    $"A SingleMessageHandler for message type {typeof(TMessage).Name} is already registered.");
            }
            _handlers[typeof(TMessage)] = new Func<ISingleMessage<TReturn>, TReturn>(
                message => handler.Invoke((TMessage) message));
        }

        public void AddWithoutReturnType<TMessage>(Action<TMessage> handler)
        {
            if (_handlers.ContainsKey(typeof(TMessage)))
            {
                throw new UniMediatorException(
                    $"A SingleMessageHandler for message type {typeof(TMessage).Name} is already registered.");
            }

            _handlers[typeof(TMessage)] = new Action<ISingleMessage>(
                message => handler.Invoke((TMessage) message));
        }

        public void Invoke(ISingleMessage message)
        {
             AotCodeGenerator.RegisterGenericValueType();

            if (!_handlers.TryGetValue(message.GetType(), out var @delegate))
            {
                throw new Exception($"No handler is registered for {message}");
            }

            if (! (@delegate is Action<ISingleMessage> action))
            {
                throw new Exception($"Handler for {message} is null");
            }
            action.Invoke(message);
        }

        public T Invoke<T>(ISingleMessage<T> message)
        {
            AotCodeGenerator<T>.RegisterGenericValueType();

            if (!_handlers.TryGetValue(message.GetType(), out var @delegate))
            {
                return MissingReturnValue<T>(
                    $"No handler returning type {typeof(T)} is registered for {message}");
            }

            if (! (@delegate is Func<ISingleMessage<T>, T> func))
            {
                return MissingReturnValue<T>(
                    $"Handler returning type {typeof(T)} for {message} is null");
            }
            
            return func.Invoke(message);
        }

        // If there is no sane value to return, throw an Exception if running in 
        // Unity Editor. Return default(T) if running outside of Editor
        private T MissingReturnValue<T>(string errorMessage)
        {
        #if UNITY_EDITOR
            throw new UniMediatorException(errorMessage);
        #else
            Debug.LogError("UniMediator: " + errorMessage);
            return default(T);
        #endif
        }
        
        public void Remove(Type messageType)
        {
            if (!_handlers.ContainsKey(messageType))
            {
                return;
            }
            _handlers.Remove(messageType);
        }

        public void CacheHandler(
            Type messageType,
            Type returnType,
            object instance, 
            MethodInfo method)
        {
            var handler = DelegateFactory.CreateSingleMessageHandler(messageType, returnType, instance, method);
             
            var genericMethod = GetType()
                .GetCachedGenericMethod(returnType == typeof(void) ? nameof(AddWithoutReturnType) : nameof(Add), BindingFlags.Instance | BindingFlags.Public, messageType, returnType);
                        
            genericMethod.Invoke(this, new[] { handler });
        }
    }
}