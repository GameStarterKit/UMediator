using System;

namespace Packages.UMediator.Runtime.Internal
{
    // Generates code for methods with generic value type arguments, that would otherwise
    // not be generated by the AOT compiler
    
    internal static class AotCodeGenerator<T>
    {
        public static void RegisterGenericValueType()
        {
        }

        private static void AotWorkAround()
        {
            new SingleMessageHandlerCache().Add<object, T>( null);
            DelegateFactory.CreateClosedFuncType<object, T>();
            throw new InvalidOperationException(
                "This method is used for AOT code generation only. Do not call it at runtime.");
        }
    }
}