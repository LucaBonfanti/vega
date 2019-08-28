using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace Vega
{
    public class Program
    {

    /*  Original 
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                .UseStartup<Startup>()
                .Build();

            host.Run();
        }
    /* */
    /*
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    */
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
            /*  .UseKestrel(options => {
                            options.Listen(System.Net.IPAddress.Loopback, 5000);  // http:localhost:5000
                            options.Listen(System.Net.IPAddress.Any, 80);         // http:*:80
                    })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration() */
                .UseStartup<Startup>();
        }
    /* */

}
