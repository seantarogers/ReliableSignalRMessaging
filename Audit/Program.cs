
namespace Audit
{
    using Topshelf;

    public class Program
    {
        public static void Main()
        {
            //HostFactory.Run(
            //    x =>
            //    {
            //        x.Service<IServiceHost>(
            //            s =>
            //            {
            //                s.ConstructUsing(pc => new ServiceHost());
            //                s.WhenStarted((pc, hostControl) => pc.Start(hostControl));
            //                s.WhenStopped(pc => pc.Stop());
            //            });
            //        x.RunAsLocalSystem();

            //        var audit = "Audit";
            //        x.SetDescription(audit);
            //        x.SetDisplayName(audit);
            //        x.SetServiceName(audit);
            //    });
        }
    }
}
