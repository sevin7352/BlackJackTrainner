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
        public static string RuleSetsLocation = "C:\\Users\\Grant.Sevin\\source\\repos\\BlackJackTrainner\\BlackJackClasses\\BalckJackRuleSets\\";
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
    }
}
