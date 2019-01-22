using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SumoLogic.Logging.AspNetCore;

namespace SumoLogic.Logging.AspNetCore.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                 // bind logger configuration from configuration
                 .ConfigureLogging((context, builder) =>
                 {
                     builder.AddSumoLogic(opts =>
                     {
                         context.Configuration.GetSection("SumoLogicLoggingOptions").Bind(opts);
                     });
                 })
                // or alternatively, manually set the options
                //.ConfigureLogging(builder => builder.AddSumoLogic(opts =>
                //{
                //    opts.Uri = "https://collectors.us2.sumologic.com/receiver/v1/http/your_endpoint_here==";
                //}))
                .UseStartup<Startup>()
                .Build();
    }
}
