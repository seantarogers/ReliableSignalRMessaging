namespace Saga
{
    using Topshelf;

    public class Program
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
                                    s.WhenStarted(((pc, control) => pc.Start(control)));
                                    s.WhenStopped(pc => pc.Stop());
                                });
                        x.RunAsLocalSystem();

                        x.SetDescription("Saga");
                        x.SetDisplayName("Saga");
                        x.SetServiceName("Saga");
                    });
        }
    }
}
