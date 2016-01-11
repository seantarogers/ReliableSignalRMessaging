namespace Audit
{
    using System;

    using Extensions;

    using Autofac;

    using Microsoft.Owin.Hosting;

    using NServiceBus;
    
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration configuration)
        {
            var container = CreateContainer();
            configuration.Configure(container);
        }

        private static void StartOwinWebHost()
        {
            const string HttpLocalhost = "http://localhost:8094";
            WebApp.Start(HttpLocalhost);
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
