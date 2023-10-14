using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using Lively.NotionGadgetsServer;
using Lively.NotionGadgetsServer.Communicator;
using Grapevine;
using Lively.NotionGadgetsServer.Models;
using Lively.NotionGadgetsServer.Settings;

namespace Lively.NotionGadgetServer
{
    public static class NotionGadgetsEntry
    {
        public static void Main()
        {
            // load API key and duplicated templae link from disk
            NotionSettings settings = SettingsSaver.LoadSettings();
            if (settings == null) return;
            else
            {
                Console.WriteLine("Loaded API key: " + settings.NotionAPISecret);
                Console.WriteLine("Duplicated template page: " + settings.NotionPage);
            }


            // Start the NotionGadgetsServer on localhost:1234
            var server = RestServerBuilder.From<Startup>().Build();
            Console.WriteLine("Starting Grapevine server");
            server.Run();
            Console.WriteLine("Press enter to stop the server");
            Console.ReadLine();
        }

    }

}
