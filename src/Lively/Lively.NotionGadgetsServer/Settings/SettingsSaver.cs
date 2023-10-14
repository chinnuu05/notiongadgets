using Lively.Common;
using Lively.NotionGadgetsServer.Models;
using Newtonsoft.Json;
using Notion.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Lively.NotionGadgetsServer.Settings
{
    public static class SettingsSaver
    {
        private static string FileName = "NotionSettings.json";
        public static NotionSettings LoadSettings()
        {
            var saveDir = Path.Combine(Constants.CommonPaths.AppDataDir, FileName);
            if (File.Exists(saveDir))
            {
                using (StreamReader r = new StreamReader(saveDir))
                {
                    string json = r.ReadToEnd();
                    NotionSettings settings = JsonConvert.DeserializeObject<NotionSettings>(json);
                    return settings;
                }
            }

            else return null;
        }
        public static void SaveSettings(string APIKey, string NotionPage)
        {
            var settings = new NotionSettings()
            {
                NotionAPISecret = APIKey,
                NotionPage = NotionPage
            };

            var jsonToSave = JsonConvert.SerializeObject(settings);
            var saveDir = Path.Combine(Constants.CommonPaths.AppDataDir, FileName);
            Console.WriteLine("Saving Notion settings: " + saveDir);
            File.WriteAllText(saveDir, jsonToSave);

        }
    }
}
