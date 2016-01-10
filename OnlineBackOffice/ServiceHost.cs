namespace OnlineBackOffice
{
    using Autofac;

    using NServiceBus;

    using Extensions;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        private static IStartableBus bus;

        public bool Start(HostControl hostControl)
        {
            var container = CreateContainer();
            var busConfiguration = new BusConfiguration();
            busConfiguration.Configure(container);

            StartBus(busConfiguration);
            return true;
        }

        public bool Stop()
        {
            if (bus != null)
            {
                bus.Dispose();
            }

            return true;
        }

        private static void StartBus(BusConfiguration busConfiguration)
        {
            bus = Bus.Create(busConfiguration);
            bus.Start();
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