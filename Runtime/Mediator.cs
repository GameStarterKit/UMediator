namespace Packages.UMediator.Runtime
{
    public sealed class Mediator
    {
        private static IMediator _mediator;

        public Mediator()
        {
            
        }
        
        public static void Publish(IMulticastMessage message)
        {
            InitDefaultImplementation();
            _mediator.Publish(message);
        }

        private static void InitDefaultImplementation()
        {
            if (_mediator == null)
            {
                _mediator = new MediatorImpl();
            }
        }
        public static T Send<T>(ISingleMessage<T> message)
        {
            InitDefaultImplementation();
            return _mediator.Send(message);
        }

        public static void InjectImplementation(IMediator mediatorImpl)
        {
            _mediator = mediatorImpl;
        }
    }
}