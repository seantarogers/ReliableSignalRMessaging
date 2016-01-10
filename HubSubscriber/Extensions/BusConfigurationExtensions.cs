namespace HubSubscriber.Extensions
{
    using System;

    using Autofac;

    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Persistence;
    using NServiceBus.Persistence.Legacy;

    public static class BusConfigurationExtensions
    {
        public static BusConfiguration Configure(this BusConfiguration busConfiguration, IContainer container)
        {
            busConfiguration.EndpointName("HubSubscriber");
            busConfiguration.UseSerialization<JsonSerializer>();

            busConfiguration.DisableFeature<Audit>();
            busConfiguration.DisableFeature<Sagas>();
            busConfiguration.DisableFeature<TimeoutManager>();

            busConfiguration.UsePersistence<MsmqPersistence, StorageType.Subscriptions>();

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
                    .And("HubSubscriber"));
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