using SimpleInjector;
using Concertina.WindowsService.InjectorExtensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using NLog;

namespace Concertina.WindowsService
{
    class Program
    {
        private static readonly Container _container = new Container();        

        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                                   .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
                                   .AddJsonFile(@"concertina.json", optional: false, reloadOnChange: true)
                                   .Build();

            _container.ConfigureContainer(configuration)
                      .RunHostFactory();


        }
    }
}
