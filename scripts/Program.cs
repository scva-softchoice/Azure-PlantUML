using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

/// <summary>
/// The main program class.
/// </summary>
public class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, configuration) =>
            {
                configuration.Sources.Clear();

                IHostEnvironment env = hostingContext.HostingEnvironment;

                configuration
                    .AddJsonFile("apsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"apsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

                configuration.AddEnvironmentVariables();
            })
            .ConfigureServices((services) =>
            {
                services.AddSingleton<IImageManager, SvgManager>();
                services.AddHostedService<GeneratePlantuml>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            })
            .Build();

        await host.RunAsync();
    }
}
