﻿using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Ocuda.Ops.Web
{
    public class Program
    {
        public static int Main(string[] args)
        {
            var applicationName = Assembly.GetExecutingAssembly().GetName().Name;
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var id = Process.GetCurrentProcess().Id;

            var webHost = CreateWebHostBuilder(args).Build();
            var config = webHost.Services.GetService(typeof(IConfiguration)) as IConfiguration;

            Log.Logger = new LogConfig().Build(config, applicationName, version, id).CreateLogger();

            var instanceConfig = config["Ops.Instance"];
            string instance = string.IsNullOrEmpty(instanceConfig) ? null : $" ({instanceConfig})";

            try
            {
                Log.Information($"{applicationName} v{version}{instance} starting up");
                webHost.Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, $"{applicationName} v{version}{instance} exited unexpectedly: {ex.Message}");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseSerilog();
    }
}
