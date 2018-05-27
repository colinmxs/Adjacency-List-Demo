using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdjacencyListDemo.Web.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AdjacencyListDemo.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            if (args.Any(arg => arg == "bootstrapdb"))
            {
                var bootstrapper = new DatabaseBootstrapper();
                bootstrapper.Run();
                Console.WriteLine("Database bootstrapping complete...");
            }
            else
                BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
