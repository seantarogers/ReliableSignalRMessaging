namespace TestMessagePublisher
{
    using System;

    using Autofac;

    using Messages.Events;

    using NServiceBus;
    using NServiceBus.Log4Net;
    using NServiceBus.Logging;

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
            const int RemoteBrokerId = 123;
            var agreementPlacedEvent = CreateAgreementPlacedEvent(RemoteBrokerId);
            bus.Publish(agreementPlacedEvent);
        }

        private static void PublishAgreementPlacedForOnlineBrokerEvent(ISendOnlyBus bus)
        {
            const int OnlineBrokerId = 456;
            var agreementPlacedEvent = CreateAgreementPlacedEvent(OnlineBrokerId);
            bus.Publish(agreementPlacedEvent);
        }

        private static AgreementPlacedEvent CreateAgreementPlacedEvent(int brokerId)
        {
            var attachmentReceivedEvent = new AgreementPlacedEvent
            {
                AgreementId = 1,
                AgreementDocumentUrl = "https://upload.wikimedia.org/wikipedia/en/f/f4/The_Best_Best_of_Fela_Kuti.jpg",
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