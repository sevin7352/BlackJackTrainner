using BlackJackClasses.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Helpers
{
    public static class BlackJackRuleSetHelper
    {
        public static string RuleSetsLocation
        {
            get
            {
                string current = AppDomain.CurrentDomain.BaseDirectory;
                string dataBaseRootFolder = current.Substring(0, current.IndexOf("BlackJackTrainner"));


                return Path.Combine(dataBaseRootFolder, "BlackJackTrainner\\BlackJackClasses", "BalckJackRuleSets");
            }
        }


        public static List<BlackJackRules> GetRuleSets()
        {
            List<BlackJackRules> ruleSetsToReturn = new List<BlackJackRules>();
            var filesToLoad = Directory.GetFiles(RuleSetsLocation);
            foreach (var file in filesToLoad)
            {
                var ruleSetToAdd = JsonConvert.DeserializeObject<BlackJackRules>(File.ReadAllText(file));
                ruleSetsToReturn.Add(ruleSetToAdd);
            }


            return ruleSetsToReturn;
        }

        public static void CreateBlankRuleSet()
        {
            var blankRuleSet = new BlackJackRules() { name = "Blank" };
            File.WriteAllText(RuleSetsLocation + blankRuleSet.name + ".json", JsonConvert.SerializeObject(blankRuleSet));
        }

        public static void SaveRuleSets(List<BlackJackRules> ruleSets)
        {
            foreach (var ruleSet in ruleSets)
            {
                File.WriteAllText(RuleSetsLocation + ruleSet.name + ".json", JsonConvert.SerializeObject(ruleSet));
            }
        }
        public static BlackJackRules AllowUserToSelectRuleSet()
        {
            var rules = BlackJackRuleSetHelper.GetRuleSets();
            int index = 1;
            Console.WriteLine("Select Rules To use by entering the corrisponding number");
            foreach (var rule in rules)
            {
                Console.WriteLine(index + " - " + rule.name);
            }

            var ruleToUse = rules[0];
            if (rules.Count != 1)
            {
                var input = Console.ReadLine();
                int selectedIndex;
                int.TryParse(input, out selectedIndex);
                if (selectedIndex != -1)
                {
                    ruleToUse = rules[selectedIndex];
                }
            }

            return ruleToUse;
        }
    }
}
