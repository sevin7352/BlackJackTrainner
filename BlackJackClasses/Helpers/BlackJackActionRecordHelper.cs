using BlackJackClasses.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlackJackClasses.Helpers
{
    public static class BlackJackActionRecordHelper
    {

        private static string FolderPath
        {
            get
            {
                string current = AppDomain.CurrentDomain.BaseDirectory;
                string dataBaseRootFolder = current.Substring(0,current.IndexOf("BlackJackTrainner"));

               
                return Path.Combine(dataBaseRootFolder,"BlackJackTrainner\\BlackJackClasses", "GameRecords");
            }
        }

        static BlackJackActionRecordHelper()
        {
            // Ensure the directory exists
            if (!Directory.Exists(FolderPath))
            {
                Directory.CreateDirectory(FolderPath);
            }
        }
        public static async Task SaveGameRecordsAsync(List<BlackJackActionRecord> records,string RulesName)
        {
            var listDistinct = records.Select(p=>p.GameId.ToString()).Distinct();
            foreach (var gameId in listDistinct)
            {
                string filePath = Path.Combine(FolderPath, RulesName);
                filePath = Path.Combine(filePath, $"BlackJackGame_{gameId}.json");

                var options = new JsonSerializerOptions { WriteIndented = true };

                string json = JsonSerializer.Serialize(records, options);
                await File.WriteAllTextAsync(filePath, json);
            }
        }

        public static async Task<List<BlackJackActionRecord>> LoadGameRecordsAsync(string RulesName,int gameId)
        {

            string filePath = Path.Combine(FolderPath, RulesName);

            if (!Directory.Exists(filePath))
                return new List<BlackJackActionRecord>();
            
            filePath = Path.Combine(filePath, $"BlackJackGame_{gameId}.json");

            if (!File.Exists(filePath))
                return new List<BlackJackActionRecord>();

            string json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<List<BlackJackActionRecord>>(json) ?? new List<BlackJackActionRecord>();
        }

        public static async Task<List<BlackJackActionRecord>> LoadAllRecordsAsync(string RulesName)
        {
            
            string filePath = Path.Combine(FolderPath, RulesName);

            if (!Directory.Exists(filePath))
                return new List<BlackJackActionRecord>();

            var files = Directory.GetFiles(filePath);
            List<BlackJackActionRecord> RecordsToReturn = new List<BlackJackActionRecord>();
            foreach (var file in files)
            {
                string json = await File.ReadAllTextAsync(file);
                RecordsToReturn.AddRange(JsonSerializer.Deserialize<List<BlackJackActionRecord>>(json));
            }

            return RecordsToReturn;
        }




    }
}
