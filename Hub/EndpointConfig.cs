
namespace Hub
{
    using System;
    using System.IO;

    using Autofac;

    using Extensions;

    using log4net.Config;

    using Microsoft.Owin.Hosting;

    using NServiceBus;
    using NServiceBus.Log4Net;
    using NServiceBus.Logging;
    
    public class EndpointConfig : IConfigureThisEndpoint
    {
        public static IContainer Container { get; set; }
        public void Customize(BusConfiguration busConfiguration)
        {
            SetUpLog4Net();
            Container = CreateContainer();
            StartOwinWebHost();

            busConfiguration.EndpointName("Hub");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.UsePersistence<NHibernatePersistence>();
            ApplyCustomConventions(busConfiguration);
            ConfigureAssembliesToScan(busConfiguration);

            busConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(Container));            
        }

        private static void ConfigureAssembliesToScan(BusConfiguration busConfiguration)
        {
            busConfiguration.AssembliesToScan(
                AllAssemblies.Matching("NServiceBus")
                    .And("Messages")
                    .And("Hub"));
        }

        private static void ApplyCustomConventions(BusConfiguration busConfiguration)
        {
            var conventions = busConfiguration.Conventions();
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Events"));
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            conventions.DefiningTimeToBeReceivedAs(
                t => t.Name.EndsWith("Expires") ? TimeSpan.FromSeconds(30) : TimeSpan.MaxValue);
        }

        private static void StartOwinWebHost()
        {
            var httpLocalhost = "http://localhost:8093";
            var webHost = WebApp.Start(httpLocalhost);
            Console.WriteLine("Successfully started the SignalR publisher on: {0}", httpLocalhost);
        }
        
        private static void SetUpLog4Net()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            LogManager.Use<Log4NetFactory>();
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
