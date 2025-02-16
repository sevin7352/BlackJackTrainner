// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Helpers;

Console.WriteLine("Hello, World!");

//Get Rule Set to simulate
var rules = BlackJackRuleSetHelper.AllowUserToSelectRuleSet();

var existingRecords = BlackJackActionRecordHelper.LoadAllRecordsAsync(rules.name);


