using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Diagnostics;
using Common.Utilities;

namespace Worker1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            if (Debugger.IsAttached || args.Contains("--debug"))
            {
                host.Run();
            }
            else
            {
                host.RunAsCustomService();
            }
        }
    }
}
