// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Helpers;

Console.WriteLine("Hello, World!");

var RuleSetToUse = BlackJackRuleSetHelper.AllowUserToSelectRuleSet();
NerualNetworkHelper.CreateTrainingDataFile(RuleSetToUse.name);
