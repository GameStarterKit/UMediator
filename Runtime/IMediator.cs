using System;
using System.Collections.Generic;
using System.Reflection;

namespace Packages.UMediator.Runtime
{
    public interface IMediator
    {
        /// <summary>
        /// Sends a message to a single handler with a non-void return type 
        /// </summary>
        /// <param name="message">The message object</param>
        /// <typeparam name="TResult">The return type of the handler</typeparam>
        /// <returns>The response of the handler</returns>
        TResult Send<TResult>(ISingleMessage<TResult> message);
        void Send(ISingleMessage message);
        
        /// <summary>
        /// Sends a message to multiple handlers with a void return type
        /// </summary>
        /// <param name="message">The message object</param>
        void Publish(IMulticastMessage message);

        void RegisterAssemblies(IEnumerable<Assembly> assemblies);
        void RegisterDiDelegate(Action<object> injectionDelegate);
        void CacheMessageHandlers();
    }
}