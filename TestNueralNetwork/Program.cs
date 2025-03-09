// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Helpers;
using BlackJackNueralNetworkLibrary.Model;
using BlackJackClasses.Enums;
using Microsoft.ML;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

Console.WriteLine("TrainModel and Save");

var RuleSetToUse = BlackJackRuleSetHelper.GetRuleSets().FirstOrDefault();
//NerualNetworkHelper.CreateTrainingDataFile(RuleSetToUse.name);

//string BaseModelsRelativePath = @"../../../../MLModels";
//string ModelRelativePath = $"{BaseModelsRelativePath}/BlackJackModel.zip";
//string ModelPath = GetAbsolutePath(ModelRelativePath);
var DataToTrainOn = NerualNetworkHelper.GetTrainingDataFromDatabse(RuleSetToUse.name);

BlackJackNueralNetModel BlackJackmodel = new BlackJackNueralNetModel(RuleSetToUse.name);
Console.WriteLine("Would You Like To start With a Fresh Model?");
var FreshModelInput = Console.ReadLine();

if (!File.Exists(BlackJackmodel.locationToSaveModelTo) || FreshModelInput.ToLower().Contains("y"))
{
    //SaveNewModel
    
    BlackJackmodel.TrainModel(DataToTrainOn);

    // Save the trained model to a file for future use
    BlackJackmodel.SaveModel();
}
    

Console.WriteLine("Would You Like To Retrain your Model?");
var input = Console.ReadLine();
bool ContinueLoop = true;
while (ContinueLoop)
{
    if (input.ToLower().Contains("y")) //retrain
    {
        BlackJackmodel.TrainOrRetrainModel(DataToTrainOn);
    }

    int NumberCorrect = 0;
    int total = DataToTrainOn.Count;
    foreach (var dataEntry in DataToTrainOn)
    {
        var actionToTake = BlackJackmodel.PredictAction(dataEntry);

        if (actionToTake == dataEntry.Action)
        {
            NumberCorrect++;
            //Console.WriteLine("Correct:" + dataEntry.DealerUpCard + ":" + dataEntry.PlayerHandSum + ":" + dataEntry.NumberOfAces);
        }
        else
        {
            Console.WriteLine("Incorrect: Acion Chosen:" + ((ActionTypes)actionToTake).ToString() + "  ActionFromData:" + ((ActionTypes)dataEntry.Action).ToString() + "\n" +
                "Dealers Card:" + dataEntry.DealerUpCard + "\nPlayersSum:" + dataEntry.PlayerHandSum + "\nNumberOFAces:" + dataEntry.NumberOfAces+"\nCanSplit:"+dataEntry.CanSplit+"\nCanDouble:"+dataEntry.CanDouble);
        }

    }
    Console.WriteLine("PercentCorrect:" + (double)NumberCorrect / total);

    Console.WriteLine("Would You Like To Continue Loop?");
    ContinueLoop = Console.ReadLine().ToLower().Contains("y");
}


//Save Before Exiting
BlackJackmodel.SaveModel();


string GetAbsolutePath(string relativePath)
{
    var _dataRoot = new FileInfo(System.IO.Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath));
    string assemblyFolderPath = _dataRoot.Directory.FullName;

    string fullPath = Path.Combine(assemblyFolderPath, relativePath);

    return fullPath;
}




