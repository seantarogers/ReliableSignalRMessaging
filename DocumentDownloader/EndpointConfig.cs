
namespace DocumentDownloader
{
    using System;
    using System.IO;

    using Autofac;

    using Extensions;

    using log4net.Config;

    using NServiceBus;
    using NServiceBus.Log4Net;
    using NServiceBus.Logging;

    public class EndpointConfig : IConfigureThisEndpoint
    {
        public void Customize(BusConfiguration busConfiguration)
        {
            var container = CreateContainer();

            SetUpLog4Net();
            busConfiguration.EndpointName("DocumentDownloader");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.UsePersistence<NHibernatePersistence>();

            busConfiguration.EnableInstallers();
            ApplyCustomConventions(busConfiguration);
            ConfigureAssembliesToScan(busConfiguration);
            

            busConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));
        }
        
        private static void ConfigureAssembliesToScan(BusConfiguration busConfiguration)
        {
            busConfiguration.AssembliesToScan(
                AllAssemblies.Matching("NServiceBus")
                    .And("Messages")
                    .And("DocumentDownloader"));
        }

        private static void ApplyCustomConventions(BusConfiguration busConfiguration)
        {
            var conventions = busConfiguration.Conventions();
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Events"));
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            conventions.DefiningTimeToBeReceivedAs(
                t => t.Name.EndsWith("Expires") ? TimeSpan.FromSeconds(30) : TimeSpan.MaxValue);
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
