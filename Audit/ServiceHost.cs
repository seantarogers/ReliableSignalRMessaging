using System;
using Autofac;
using Microsoft.Owin.Hosting;
using NServiceBus;
using Topshelf;
using Audit.Extensions;

namespace Audit
{
    public class ServiceHost : IServiceHost
    {
        private static IStartableBus bus;

        private static IDisposable webHost;

        public static IContainer Container { get; private set; }

        public bool Start(HostControl hostControl)
        {
            Container = CreateContainer();
            var busConfiguration = new BusConfiguration();
            busConfiguration.Configure(Container);

            StartBus(busConfiguration);
            StartOwinWebHost();
            return true;
        }

        public bool Stop()
        {
            if (bus != null)
            {
                bus.Dispose();
            }

            if (webHost != null)
            {
                webHost.Dispose();
            }

            return true;
        }
        
        private static void StartBus(BusConfiguration busConfiguration)
        {
            bus = Bus.Create(busConfiguration);
            bus.Start();
        }
        
        private static void StartOwinWebHost()
        {
            const string HttpLocalhost = "http://localhost:8094";
            webHost = WebApp.Start(HttpLocalhost);
            Console.WriteLine("Successfully started the SignalR publisher on: {0}", HttpLocalhost);
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