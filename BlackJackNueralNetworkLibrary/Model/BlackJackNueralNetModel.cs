using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackJackClasses.Enums;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlackJackNueralNetworkLibrary.Model
{
    public class BlackJackNueralNetModel
    {
        public BlackJackNueralNetModel(string rulesName)
        {
            mlContext = new MLContext();
            RulesName = rulesName;
            LoadModel();
        }

        public string RulesName { get; set; }
        public ITransformer Model;
        public string locationtoSaveToBase => "C:\\Users\\Grant.Sevin\\Source\\Repos\\BlackJackTrainner\\BlackJackNueralNetworkLibrary\\NueralNetworks\\" + RulesName;
        public string locationToSaveModelTo =>  locationtoSaveToBase+ "-BlackJackModel.zip";
        public string TrainingLoopStatsFile => locationtoSaveToBase + "-loopResults.csv";
        private MLContext mlContext { get; set; }

        // Save the model to a file
        public void SaveModel()
        {
            mlContext.Model.Save(Model, null, locationToSaveModelTo);
            Console.WriteLine($"Model saved to: {locationToSaveModelTo}");
        }

        // Load the model from a file
        public ITransformer LoadModel()
        {
            if (!File.Exists(locationToSaveModelTo)) {
                return null;
            }
           
            DataViewSchema modelInputSchema;
            ITransformer model = mlContext.Model.Load(locationToSaveModelTo, out modelInputSchema);
            Console.WriteLine($"Model loaded from: {locationToSaveModelTo}");
            Model = model;
            PredictionEngine = mlContext.Model.CreatePredictionEngine<NueralNetTrainingDataEntry, NueralNetworkOutput>(Model);
            return model;
        }

        public ITransformer TrainModel(List<NueralNetTrainingDataEntry> data)
        {

            // Load data into IDataView format
            var trainData = mlContext.Data.LoadFromEnumerable(data);

            // Define a pipeline: Features = PlayerHandSum, DealerUpcard, NumberOfAces
            var pipeline = GetPipeline();

            // Train the model
            var model = pipeline.Fit(trainData);
            PredictionEngine = mlContext.Model.CreatePredictionEngine<NueralNetTrainingDataEntry, NueralNetworkOutput>(Model);
            return model;
        }
        public ITransformer TrainOrRetrainModel(List<NueralNetTrainingDataEntry> data)
        {
            ITransformer model;

            if (File.Exists(locationToSaveModelTo))
            {
                Console.WriteLine("Loading existing model...");
                model = LoadModel();
            }
            else
            {
                Console.WriteLine("No existing model found. Training a new one...");
                model = TrainModel(data);
            }

            // Retrain the model with new data
            var trainData = mlContext.Data.LoadFromEnumerable(data);
            // Define the full training pipeline
            var pipeline = GetPipeline(); // Convert back to int

            //var retrainedModel = model.Transform(trainData); // Apply the transformations

            Model = pipeline.Fit(trainData);

            // Save the retrained model
            SaveModel();

            PredictionEngine = mlContext.Model.CreatePredictionEngine<NueralNetTrainingDataEntry, NueralNetworkOutput>(Model);
            return Model;
        }

        public PredictionEngine<NueralNetTrainingDataEntry,NueralNetworkOutput> PredictionEngine { get; set; }

        public int PredictAction(NueralNetworkInput gameState)
        {
            NueralNetTrainingDataEntry trainData = (NueralNetTrainingDataEntry)gameState;
            
            // Predict the action for the given game state
            var prediction = PredictionEngine.Predict(trainData);
            var predictedActions = prediction.ActionScores
                    .Select((score, index) => new { score, index })
                    .OrderByDescending(x => x.score);

            foreach (var action in predictedActions) {

                switch ((ActionTypes)action.index) 
                {
                    case ActionTypes.Split:
                        if (gameState.CanSplit ==1)
                        {
                            return (int)ActionTypes.Split;
                        }
                        break;
                    case ActionTypes.Double:
                        if (gameState.CanDouble == 1)
                        {
                            return (int)ActionTypes.Double;
                        }
                        break;
                    case ActionTypes.Stay:
                        return (int)ActionTypes.Stay;
                        break;
                    case ActionTypes.Hit:
                        return (int)ActionTypes.Hit;
                        break;
                }
            
            
            }
            
            // Choose the action with the highest score (the model's output)
            return (int)ActionTypes.Stay;
        }

        public EstimatorChain<KeyToValueMappingTransformer> GetPipeline()
        {
            return mlContext.Transforms.Conversion.MapValueToKey("Label", "Action") // Convert Action to categorical key and rename it to "Label"
        .Append(mlContext.Transforms.Conversion.ConvertType("PlayerHandSum", "PlayerHandSum", DataKind.Single))
        .Append(mlContext.Transforms.Conversion.ConvertType("DealerUpCard", "DealerUpCard", DataKind.Single))
        .Append(mlContext.Transforms.Conversion.ConvertType("NumberOfAces", "NumberOfAces", DataKind.Single))
        .Append(mlContext.Transforms.Conversion.ConvertType("CanSplit", "CanSplit", DataKind.Single))
        .Append(mlContext.Transforms.Conversion.ConvertType("CanDouble", "CanDouble", DataKind.Single))
        .Append(mlContext.Transforms.Conversion.ConvertType("DeckCount", "DeckCount", DataKind.Single))
        .Append(mlContext.Transforms.Concatenate("Features", "PlayerHandSum", "DealerUpCard", "NumberOfAces", "CanSplit", "CanDouble", "DeckCount"))
        .Append(mlContext.MulticlassClassification.Trainers.OneVersusAll(
            mlContext.BinaryClassification.Trainers.FastTree(
                numberOfLeaves: 50,
                numberOfTrees: 200,
                learningRate: 0.1
            ), labelColumnName: "Label")) // Ensure model uses "Label" instead of "Action"
        .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedAction", "PredictedLabel")); // Convert back to original label values
        }
    }
}
