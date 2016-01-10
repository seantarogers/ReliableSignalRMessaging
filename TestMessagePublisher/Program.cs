namespace TestMessagePublisher
{
    using System;

    using Autofac;

    using Messages.Events;

    using NHibernate.Cfg;

    using NServiceBus;
    using NServiceBus.Log4Net;
    using NServiceBus.Logging;
    using NServiceBus.Persistence;

    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Test Message Publisher");
            Console.WriteLine("==============================");
            var container = SetUpContainer();
            var busConfiguration = SetUpBusConfiguration(container);
            using (var bus = Bus.Create(busConfiguration).Start())
            {
                Console.WriteLine("1. enter 'apo' to publish an AgreementPlacedEvent from an online customer to the Bus");
                Console.WriteLine("2. enter 'apr' to publish an AgreementPlacedEvent from a remote customer to the Bus");
                Console.WriteLine("3. enter 'e' to exit");

                string input;
                while ((input = Console.ReadLine()) != "e")
                {
                    if (input == "apo")
                    {
                        PublishAgreementPlacedForOnlineBrokerEvent(bus);
                        Console.WriteLine("==============================");
                        Console.WriteLine("Published: online AgreementPlacedEvent to the saga queue");
                    }

                    if (input == "apr")
                    {
                        PublishAgreementPlacedForRemoteBrokerEvent(bus);
                        Console.WriteLine("==============================");
                        Console.WriteLine("Published: remote AgreementPlacedEvent to the saga queue");
                    }
                }

                if (input == "e")
                {
                    return;
                }
                Console.ReadLine();
            }
        }

        private static void PublishAgreementPlacedForRemoteBrokerEvent(ISendOnlyBus bus)
        {
            var agreementPlacedEvent = CreateAgreementPlacedEvent(123);
            bus.Publish(agreementPlacedEvent);
        }

        private static void PublishAgreementPlacedForOnlineBrokerEvent(ISendOnlyBus bus)
        {
            var agreementPlacedEvent = CreateAgreementPlacedEvent(456);
            bus.Publish(agreementPlacedEvent);
        }

        private static AgreementPlacedEvent CreateAgreementPlacedEvent(int brokerId)
        {
            var attachmentReceivedEvent = new AgreementPlacedEvent
            {
                AgreementId = 1,
                AgreementDocumentUrl = "http://static1.squarespace.com/static/536a4606e4b065f2a294b1f1/t/5659aa07e4b0b807739aea9c/1448716807468/riddles+dessert+Dec2015+single.pdf",
                CorrelationId = Guid.NewGuid(),
                BrokerId = brokerId
            };
            return attachmentReceivedEvent;
        }

        private static BusConfiguration SetUpBusConfiguration(ILifetimeScope container)
        {
            LogManager.Use<Log4NetFactory>();
            var busConfiguration = new BusConfiguration();
            busConfiguration.EndpointName("testmessagepublisher");
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.UsePersistence<NHibernatePersistence>();

            var conventions = busConfiguration.Conventions();
            conventions.DefiningEventsAs(t => t.Namespace != null && t.Namespace.Contains("Events"));
            conventions.DefiningCommandsAs(t => t.Namespace != null && t.Namespace.Contains("Commands"));

            busConfiguration.EnableInstallers();
            return busConfiguration;
        }

        private static IContainer SetUpContainer()
        {
            var containerBuilder = new ContainerBuilder();
            return containerBuilder.Build();
        }
    }
}