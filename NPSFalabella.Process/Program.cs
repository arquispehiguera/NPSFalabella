using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NPSFalabella.Core.Interfaces;
using NPSFalabella.Infraestructure.Data;
using NPSFalabella.Infraestructure.Repositories;
using Serilog;
using System;
using System.IO;
using System.Threading;

namespace NPSFalabella.Process
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
                Log.Logger = new LoggerConfiguration()
                    .Enrich.WithClientAgent()
                    .Enrich.WithClientIp()
                    .ReadFrom.Configuration(configuration).CreateLogger();

                try
                {
                    var host = CreateHostBuilder(args).Build();
                    host.Services.GetService<INps>().ProcesarTrama().Wait();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error General " + ex.Message);
                }
                Thread.Sleep(10000);
            }
        }
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var hostBuilder = Host.CreateDefaultBuilder(args)
                .UseSerilog()
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
                    
                })
                .ConfigureServices((context, services) =>
                {
                    
                    services.AddTransient<INps, Nps>();
                    services.AddSingleton<DbContextApp>();
                });

            return hostBuilder;
        }
    }
}
