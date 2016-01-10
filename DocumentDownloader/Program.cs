namespace DocumentDownloader
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
                              s.WhenStarted((pc, hostControl) => pc.Start(hostControl));
                              s.WhenStopped(pc => pc.Stop());
                          });
                      x.RunAsLocalSystem();

                      var documentdownloader = "DocumentDownloader";
                      x.SetDescription(documentdownloader);
                      x.SetDisplayName(documentdownloader);
                      x.SetServiceName(documentdownloader);
                  });
        }
    }
}
