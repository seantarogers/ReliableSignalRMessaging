
namespace Audit
{
    using System;
    using System.IO;

    using Extensions;

    using Autofac;

    using log4net.Config;

    using Microsoft.Owin.Hosting;

    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Log4Net;
    using NServiceBus.Logging;

    public class EndpointConfig : IConfigureThisEndpoint, AsA_Server
    {
        public static IContainer Container { get; private set; }

        public void Customize(BusConfiguration busConfiguration)
        {
            Container = CreateContainer();

            SetUpLog4Net();
            busConfiguration.EndpointName("Audit");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.DisableFeature<Audit>();
            busConfiguration.UsePersistence<NHibernatePersistence>();

            busConfiguration.EnableInstallers();
            ApplyCustomConventions(busConfiguration);
            ConfigureAssembliesToScan(busConfiguration);
            busConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(Container));

            StartOwinWebHost();
        }

        private static void ConfigureAssembliesToScan(BusConfiguration busConfiguration)
        {
            busConfiguration.AssembliesToScan(
                AllAssemblies.Matching("NServiceBus")
                    .And("Messages")
                    .And("Audit"));
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
            const string HttpLocalhost = "http://localhost:8094";
            var webHost = WebApp.Start(HttpLocalhost);
            Console.WriteLine("Successfully started audit web host on: {0}", HttpLocalhost);
        }

        private static IContainer CreateContainer()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterComponents();
            var container = containerBuilder.Build();
            return container;
        }


        private static void SetUpLog4Net()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config"));
            LogManager.Use<Log4NetFactory>();
        }
    }
}
