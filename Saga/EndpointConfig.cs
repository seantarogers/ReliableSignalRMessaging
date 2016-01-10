
namespace Saga
{
    using Autofac;

    using NServiceBus;

    using Extensions;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            var container = CreateContainer();
            var busConfiguration = new BusConfiguration();
            busConfiguration.Configure(container);
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterComponents();
            var container = containerBuilder.Build();
            return container;
        }
    }
}