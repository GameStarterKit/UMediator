using System;
using System.Reflection;
using static System.Reflection.BindingFlags;

namespace Packages.UMediator.Runtime.Internal
{
    internal static class DelegateFactory
    {
        internal static Delegate CreateMultiCastHandler(
            Type messageType,
            object instance, 
            MethodInfo method)
        {
            var methodInfo = typeof(DelegateFactory).GetCachedGenericMethod(nameof(CreateClosedActionType), Static | NonPublic, messageType);
            var delegateType = methodInfo.Invoke(null, null);
            return Delegate.CreateDelegate((Type)delegateType, instance, method);
        }
        
        internal static Delegate CreateSingleMessageHandler(
            Type messageType,
            Type returnType,
            object instance, 
            MethodInfo method)
        {
            var methodInfo = typeof(DelegateFactory).GetCachedGenericMethod(nameof(CreateClosedFuncType), Static | NonPublic, messageType, returnType);
            var delegateType = methodInfo.Invoke(null, null);
            return Delegate.CreateDelegate((Type)delegateType, instance, method);
        }

        private static Type CreateClosedActionType<TMessage>()
        {
            return typeof(Action<TMessage>);
        }
        
        internal static Type CreateClosedFuncType<TMessage, TResult>()
        {
            return typeof(Func<TMessage, TResult>);
        }
    }
}