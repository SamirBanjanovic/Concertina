using System;
using System.Collections.Generic;
using System.Linq;
using Concertina.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using SimpleInjector;
using Topshelf;
using Topshelf.SimpleInjector;


namespace Concertina.WindowsService.InjectorExtensions
{
    public static class SimpleInjectorExtensions
    {
        public static Container ConfigureContainer(this Container container, IConfigurationRoot configuration)
        {
            container.Register<IConcertinaService, ConcertinaService>(Lifestyle.Singleton);

            container.Register<IConcertinaConfiguration, ConcertinaConfiguration>(Lifestyle.Singleton);           

            container.Register<IPluginManager, PluginManager>(Lifestyle.Singleton);

            container.Register<IConfigurationRoot>(() => configuration, Lifestyle.Singleton);

            container.Register<ILoggerFactory>(() =>
            {
                var loggerFactory = new LoggerFactory();
                loggerFactory
                    .AddNLog()
                    .ConfigureNLog(@"nlog.config");

                return loggerFactory;
            },
            Lifestyle.Singleton);

            container.Verify();

            return container;
        }

        public static void RunHostFactory(this Container container)
        {
            var logger = container.GetInstance<ILoggerFactory>()
                                          .CreateLogger<Program>();
            try
            {
                logger.LogInformation("Starting HostFactory using SimpleInjector");
                HostFactory.Run(configuration =>
                {
                    // set working directory else it defaults to C:\Windows\System32...
                    System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);

                    var settings = container.GetInstance<IConfigurationRoot>()
                                            .GetSection("ServiceSettings");

                    configuration.SetServiceName(settings["Name"]);
                    configuration.SetDisplayName(settings["DiplayName"]);
                    configuration.SetDescription(settings["Description"]);

                    configuration.UseSimpleInjector(container)
                        .Service<IConcertinaService>(sc =>
                        {
                            sc.ConstructUsingSimpleInjector();
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
