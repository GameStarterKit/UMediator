namespace Packages.UMediator.Runtime
{
    public interface ISingleMessageHandler<in TMessage> 
        where TMessage : ISingleMessage
    {
        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="message">The message being handled</param>
        void Handle(TMessage message);
    }

    /// <summary>
    /// Defines a Handler for a message of type <see cref="ISingleMessage{T}"/> 
    /// </summary>
    /// <typeparam name="TMessage">The Type of the message parameter for this handler</typeparam>
    /// <typeparam name="TResponse">The return Type of the handler method</typeparam>
    public interface ISingleMessageHandler<in TMessage, out TResponse>
        where TMessage : ISingleMessage<TResponse> 
    {
        /// <summary>
        /// Handle the message
        /// </summary>
        /// <param name="message">The message being handled</param>
        /// <returns>Response from the handler</returns>
        new TResponse Handle(TMessage message);
    }
}