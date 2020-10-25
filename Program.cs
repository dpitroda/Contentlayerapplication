using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DAL;
using DALServices;
using LoggingService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace ContentApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {   
                    //these objects are created as dbinitailizer class as passed constructor parameters 
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    var dpcontext = services.GetRequiredService<DataProtectionKeysContext>();
                    var functionSvc = services.GetRequiredService<IFunctionalSvc>();

                    DbContextInitializer.Initialize(dpcontext, context, functionSvc)
                        .Wait();
                }
                catch (Exception ex)
                {
                    Log.Error("An error occurred while seeding the database  {Error} {StackTrace} {InnerException} {Source}",
                     ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                }
            }
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {       
                    //logfile properties that can be used or not used 
                    webBuilder.UseSerilog((hostingContext, loggingconfiguration) => loggingconfiguration
                     .Enrich.FromLogContext()
                      .Enrich.WithProperty("Application", "ContentApp")
                      .Enrich.WithProperty("MachineName", Environment.MachineName)
                      .Enrich.WithProperty("CurrentManagedThreadId", Environment.CurrentManagedThreadId)
                      .Enrich.WithProperty("OSVersion", Environment.OSVersion)
                      .Enrich.WithProperty("Version", Environment.Version)
                      .Enrich.WithProperty("UserName", Environment.UserName)
                      .Enrich.WithProperty("ProcessId", Process.GetCurrentProcess().Id)
                      .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
                      //.WriteTo.Console(theme: AnsiConsoleTheme.Code)
                      //Line above is the log file and path created automatically in the main project as text file
                      .WriteTo.File(formatter: new CustomTextFormatter(), path: Path.Combine(hostingContext.HostingEnvironment.ContentRootPath + $"{Path.DirectorySeparatorChar}Logs{Path.DirectorySeparatorChar}", $"cms_core_ng_{DateTime.Now:yyyyMMdd}.txt"))
                     .ReadFrom.Configuration(hostingContext.Configuration));
                    webBuilder.UseStartup<Startup>();
                });
    }
}
