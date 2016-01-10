namespace Hub
{
    using Topshelf;

    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(
                x =>
                {
                    x.Service<IServiceHost>(
                        s =>
                        {
                            s.ConstructUsing(pc => new ServiceHost());
                            s.WhenStarted((pc, hostControl) => pc.Start(hostControl));
                            s.WhenStopped(pc => pc.Stop());
                        });
                    x.RunAsLocalSystem();

                    x.SetDescription("Hub");
                    x.SetDisplayName("Hub");
                    x.SetServiceName("Hub");
                });
        }
    }
}
