namespace Hub.Extensions
{
    using System;
    using System.Configuration;

    using Autofac;

    using NServiceBus;
    using NServiceBus.Persistence;
    using Configuration = NHibernate.Cfg.Configuration;

    public static class BusConfigurationExtensions
    {
        public static BusConfiguration Configure(
            this BusConfiguration busConfiguration, 
            IContainer container)
        {
            busConfiguration.EndpointName("Hub");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.UsePersistence<NHibernatePersistence>();
            ApplyCustomConventions(busConfiguration);
            ConfigureAssembliesToScan(busConfiguration);

            busConfiguration.UseContainer<AutofacBuilder>(c => c.ExistingLifetimeScope(container));
            return busConfiguration;
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
    }
}