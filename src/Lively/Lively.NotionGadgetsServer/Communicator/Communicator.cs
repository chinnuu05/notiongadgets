using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Notion.Client;
using Lively.NotionGadgetsServer.Models;


namespace Lively.NotionGadgetsServer.Communicator
{
    // communicates with Notion API
    public class APICommunicator
    {
        private string APISecret = "secret_NmFirTNPqTI15HsBmCfMCEU0rAIZ0uJifHgxG5OFj7J";
        private readonly NotionClient NotionClient;

        /* Toggle IDs */
        private string TaskTogglBlockID = "e66832522be249c5a4dfcf53821ee81b"; 



        public APICommunicator()
        {


            NotionClient = NotionClientFactory.Create(new ClientOptions
            {
                AuthToken = APISecret
            });


            //var configuration = new ConfigurationBuilder()
            //.AddJsonFile("C:\\Users\\Praneeth\\source\\repos\\NotionGadgetsServer\\NotionGadgetsServer\\Communicator\\appsettings.json")
            //.Build();

            //var factory = LoggerFactory.Create(builder =>
            //{
            //    builder.ClearProviders();
            //    builder.AddConfiguration(configuration.GetSection("Logging"));
            //    builder.AddConsole();
            //});
            //NotionClientLogging.ConfigureLogger(factory);

        }

        private async Task<List<IBlock>> FetchTaskBlock()
        {
            // retrieve sync block from toggle
            var toggleChildren = await NotionClient.Blocks.RetrieveChildrenAsync(TaskTogglBlockID);

            // confirm the toggle's first block is a sync block
            var syncBlock = toggleChildren.Results.First();

            if (syncBlock.Type != BlockType.SyncedBlock)
            {
                throw new Exception("Block inside the toggle was not a sync block");
            }

            // pull to-do's from the sync block
            var syncChildren = await NotionClient.Blocks.RetrieveChildrenAsync(syncBlock.Id);


            // confirm they're to-dos
            if (syncChildren.Results.First().Type != BlockType.ToDo)
            {
                throw new Exception("Block inside sync was not a to-do block");
            }

            return syncChildren.Results;
        }

        public async Task<string> UpdateTaskBlock(string blockID, bool isChecked, string error = "No-Error")
        {
            ToDoUpdateBlock update = new ToDoUpdateBlock()
            {
                ToDo = new ToDoUpdateBlock.Info()
                {
                    IsChecked = isChecked,
                }
            };

            try
            {

                var response = await NotionClient.Blocks.UpdateAsync(blockID, update);
                var todo = (ToDoBlock)response;

                if (error != "No-Error")
                {
                    Console.WriteLine("retry: " + todo.ToDo.RichText.First().PlainText + " uiChecked: " + isChecked + " notionChecked: " + todo.ToDo.IsChecked);

                }

                return error;
            }

            // BadGateway ex.StatusCode
            // ConflictError ex.NotionAPIErrorCode
            catch (Notion.Client.NotionApiException ex)
            {
                Console.WriteLine("error");
                if (ex.StatusCode == System.Net.HttpStatusCode.BadGateway)
                {
                    Console.WriteLine("badgateway");
                    return await UpdateTaskBlock(blockID, isChecked, error="BadGateway");

                }

                else
                {
                    Console.WriteLine(ex.NotionAPIErrorCode);
                    return await UpdateTaskBlock(blockID, isChecked, error=ex.NotionAPIErrorCode.ToString());
                }

            }




        }

        public async Task<List<TaskBlock>> LoadTasks()
        {
            List<TaskBlock> tasks = new List<TaskBlock>();

            Console.ForegroundColor = ConsoleColor.Red;

            var taskBlock = FetchTaskBlock().Result;

            foreach (ToDoBlock task in taskBlock)
            {
                // add each to a dict with their plain text and isChecked bool values
                TaskBlock t = new TaskBlock()
                {
                    PlainText = task.ToDo.RichText.First().PlainText,
                    IsChecked = task.ToDo.IsChecked,
                    ID = task.Id
                };

                tasks.Add(t);
            }

            if (tasks.Count > 0)
            {
                return tasks;
            }

            return null;
        }

        public async Task<string> AddRow(string title)
        {
            string db = "5a31016d67cb4782b0c833b88ce9e75b";

            var body = PagesCreateParametersBuilder.Create(new DatabaseParentInput()
            {
                DatabaseId = db
            }).AddProperty("Name", new TitlePropertyValue
            {
                Title = new List<RichTextBase>
                {
                    new RichTextTextInput { Text = new Text { Content=title }}
                }
            }).Build();

            await NotionClient.Pages.CreateAsync(body);
            return "feuckas";
        }


        public async Task<string> Init()
        {

            // create the ToDo update block
            ToDoUpdateBlock requestParams = new ToDoUpdateBlock()
            {
                ToDo = new ToDoUpdateBlock.Info()
                {
                    RichText = new List<RichTextBaseInput>() { new RichTextTextInput()
                    {
                        Text = new Text { Content= "blH BLh"}
                    }},
                    IsChecked = true
                }
            };
            Console.WriteLine("updating to do block");

            NotionClient.Blocks.UpdateAsync("5555e98a7f844c6fac985a9612d69447", requestParams);

            return "fuck";
            //SyncedBlockUpdateBlock sync = new SyncedBlockUpdateBlock();
            //SyncedBlockBlock block = (SyncedBlockBlock)await NotionClient.Blocks.UpdateAsync("f7434a79c99943d995c82af235f54b81", sync);
            //Console.WriteLine(block.SyncedBlock.Children.First());
            //ParagraphBlock para = (ParagraphBlock)await NotionClient.Blocks.RetrieveAsync("58a11de9f7554ae7b0f5bae17b7eb533");
            //return "fuck";


        }





    }
}
