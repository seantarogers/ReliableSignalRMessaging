namespace Audit.Extensions
{
    using System;

    using Autofac;

    using NServiceBus;
    using NServiceBus.Features;

    public static class BusConfigurationExtensions
    {
        public static BusConfiguration Configure(this BusConfiguration busConfiguration, IContainer container)
        {
            busConfiguration.EndpointName("Audit");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.DisableFeature<Audit>();
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
    }
}