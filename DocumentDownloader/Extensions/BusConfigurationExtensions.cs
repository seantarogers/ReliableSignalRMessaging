namespace DocumentDownloader.Extensions
{
    using System;

    using Autofac;

    using NServiceBus;

    public static class BusConfigurationExtensions
    {
        public static BusConfiguration Configure(this BusConfiguration busConfiguration, IContainer container)
        {
            busConfiguration.EndpointName("DocumentDownloader");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.UsePersistence<NHibernatePersistence>();

            busConfiguration.EnableInstallers();
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
    }
}