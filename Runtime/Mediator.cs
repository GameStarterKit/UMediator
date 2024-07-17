using System;
using System.Collections.Generic;
using System.Reflection;

namespace Packages.UMediator.Runtime
{
    public sealed class Mediator
    {
        private static IMediator _mediator;
        private static List<Assembly> _registeredAssemblies;

        public static IEnumerable<Assembly> RegisteredAssemblies => _registeredAssemblies;

        public static void Publish(IMulticastMessage message)
        {
            InitDefaultImplementation();
            _mediator.Publish(message);
        }

        private static void InitDefaultImplementation()
        {
            if (_mediator == null)
            {
                _mediator = new MediatorImpl(_registeredAssemblies);
            }
        }
        
        public static void Send(ISingleMessage message)
        {
            InitDefaultImplementation();
            _mediator.Send(message);
        }
        
        public static T Send<T>(ISingleMessage<T> message)
        {
            InitDefaultImplementation();
            return _mediator.Send(message);
        }

        public static void InjectImplementation(IMediator mediatorImpl)
        {
            if (_mediator != null)
            {
                throw new Exception(
                    $"An {nameof(IMediator)} has been injected already or the default implementation is in use. Make sure to inject an implementation before using the Mediator.");
            }
            // TODO: implement a way to pass the registered assemblies to the custom implementations
            _mediator = mediatorImpl;
        }
        
        public static void RegisterAssembly(Assembly assembly)
        {
            _registeredAssemblies ??= new List<Assembly> {assembly};
            _registeredAssemblies.Add(assembly);
        }
    }
}