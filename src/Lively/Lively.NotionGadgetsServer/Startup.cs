using Grapevine;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lively.NotionGadgetsServer
{
    public class Startup
    {
        /*
        * Include a method with this signature (method name does not matter) if
        * you want to use an IServiceCollection implementation other than the one
        * provided by Microsoft. You can choose to configure some services here
        * as well, if you'd like.
        */
        public IServiceCollection GetServices()
        {
            return new ServiceCollection();
        }

        /*
        * Include a method with this signature (method name does not matter) to
        * configure your services. Prior to the method being called, implementations
        * for IRestServer, IRouter and IRouteScanner have already been registered.
        */
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
            });
        }

        //public void ConfigureServices(IServiceCollection services)
        //{
        //    services.Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace);
        //}
        /*
        * Include a method with this signature (method name does not matter) to
        * configure your IRestServer. Add event handlers for stopping and starting
        * the server, request recieved, and before and after routing. If you want
        * to do manual route registration (more complex) this is this place to do it.
        */
        public void ConfigureServer(IRestServer server)
        {
            server.Prefixes.Add("http://localhost:1234/");
        }
    }
}
