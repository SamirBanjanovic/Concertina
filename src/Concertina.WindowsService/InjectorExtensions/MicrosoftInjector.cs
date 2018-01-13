using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integration.Core;
using Integration.Engine;
using Integration.Engine.JobSubmitters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection.Abstractions;
using NLog.Extensions.Logging;
using Topshelf;
using ICore = Integration.Core;
using Topshelf.MicrosoftDependencyInjection;
using Integration.Egine.JobSubmitters;
using Integration.Data;

namespace Integration.WindowsService.InjectorExtensions
{
    public static class MicrosoftInjector
    {
        public static ServiceProvider ConfigureServices(this ServiceCollection services, IConfigurationRoot configuration)
        {
            services.AddSingleton<IIntegrationService, IntegrationService>();

            services.AddSingleton<ICore.IConfiguration, Configuration>();

            services.AddSingleton<IDataService>(x => new DapperDataService(null));

            services.AddSingleton<ICore.ISubmitJob, SubmitJobViaRabbitMq>();

            services.AddSingleton<IConfigurationRoot>(x => configuration);

            services.AddSingleton<ILoggerFactory>((serviceProvider) =>
            {
                var loggerFactory = new LoggerFactory();
                loggerFactory
                    .AddNLog()
                    .ConfigureNLog(@"nlog.config");

                return loggerFactory;
            });

            services.AddLogging();
            
            return services.BuildServiceProvider();
        }

        public static void RunHostFactory(this ServiceProvider provider)
        {
            var logger = provider.GetService<ILoggerFactory>()
                                 .CreateLogger<Program>();
            try
            {
                logger.LogInformation("Starting HostFactory using Microsoft Dependency Injection");
                HostFactory.Run(configuration =>
                {
                    // set working directory else it defaults to C:\Windows\System32...
                    System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

                    var settings = provider.GetService<IConfigurationRoot>()
                        .GetSection("ServiceSettings");

                    configuration.SetServiceName(settings["Name"]);
                    configuration.SetDisplayName(settings["DiplayName"]);
                    configuration.SetDescription(settings["Description"]);

                    configuration.UseMicrosoftDependencyInjection(provider)
                        .Service<IIntegrationService>(sc =>
                        {
                            sc.ConstructUsingMicrosoftDependencyInjection();
                            sc.WhenStarted(x => x.Start());
                            sc.WhenStopped(x => x.Stop());
                        })
                        .RunAsLocalSystem();
                });
            }
            catch (Exception e)
            {
                logger.LogError(e, "HostFactory encountered an error");
            }
        }
    }
}
