namespace HubSubscriber
{
    using System;

    using Autofac;

    using Extensions;

    using Managers;

    using NServiceBus;

    using Topshelf;

    public class ServiceHost : IServiceHost
    {
        public static IContainer Container { get; private set; }

        private static IStartableBus bus;

        private IHubConnectionManager hubConnectionManager;
        
        public bool Start(HostControl hostControl)
        {
            Console.WriteLine("starting hub subscriber");
            Container = CreateContainer();
            var busConfiguration = new BusConfiguration();
            busConfiguration.Configure(Container);

            StartBus(busConfiguration);
            StartConnectionToHub();

            return true;
        }
        
        private static void StartBus(BusConfiguration busConfiguration)
        {
            bus = Bus.Create(busConfiguration);
            bus.Start();
        }

        public bool Stop()
        {
            if (bus != null)
            {
                bus.Dispose();
            }

            return true;
        }

        private  void StartConnectionToHub()
        {
            hubConnectionManager = Container.Resolve<IHubConnectionManager>();
            hubConnectionManager.Start();
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



