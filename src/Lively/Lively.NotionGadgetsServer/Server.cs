using System;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using Notion.Client;
using Lively.NotionGadgetsServer.Communicator;
using Newtonsoft.Json;
using System.Web;
using System.IO;
using Lively.NotionGadgetsServer.Models;
using System.Runtime.CompilerServices;
using System.Text.Encodings.Web;

namespace Lively.NotionGadgetsServer
{
    // RESTful API class to serve data from Communicator to Client (index.html)
    [RestResource]
    public class NotionServer
    {
        private readonly APICommunicator Communicator;
        public NotionServer()
        {
            // Create APICommunicator which will talk to the Notion API
            Console.WriteLine("NotionServer constructor()");
            Communicator = new APICommunicator();
        }

        private IHttpContext DisableCORS(IHttpContext context)
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
            return context;
        }

        [RestRoute("Get", "/updatetasks/{blockID}")]
        public async Task UpdateTasks(IHttpContext context)
        {
            string fuckthisauthor = HttpUtility.UrlDecode(context.Request.PathParameters["blockID"]);


            string blockID = fuckthisauthor.Split("|")[0];
            bool isChecked = Convert.ToBoolean(fuckthisauthor.Split('|')[1]);


            string result = Communicator.UpdateTaskBlock(blockID, isChecked).Result;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.ResetColor();

            context = DisableCORS(context);
            await context.Response.SendResponseAsync(result);

        }


        [RestRoute("Get", "/tasks")]
        public async Task LoadTasks(IHttpContext context)
        {
            List<TaskBlock> tasks = Communicator.LoadTasks().Result;

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(tasks);

            context = DisableCORS(context);
            await context.Response.SendResponseAsync(json);
        }

        // load initial data
        [RestRoute("Get", "/init")]
        public async Task Init(IHttpContext context)
        {
            Console.WriteLine("loading tasks");
            Communicator.LoadTasks().Wait();
            context = DisableCORS(context);
            await context.Response.SendResponseAsync("...");
        }



        // add "Title" property to database
        [RestRoute("Post", "/addtodb/")]
        public async Task AddToDB(IHttpContext context)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("got add to db API request");

            Stream s = context.Request.InputStream as Stream;
            string text;

            using (var reader = new StreamReader(s))
            {
                text = reader.ReadToEnd();
            }

            Communicator.AddRow(text);
            context = DisableCORS(context);
            await context.Response.SendResponseAsync("{\"ur good\": \"norelaly\"}");
        }




        [RestRoute("Get", "/test")]
        public async Task Test(IHttpContext context)
        {
            context = DisableCORS(context);
            await context.Response.SendResponseAsync("Successfully hit the test route!");
        }

    }
}
