using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackJackTrainner.Enums;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BlackJackNueralNetworkLibrary.Model
{
    public class BlackJackNueralNetModel
    {
        public BlackJackNueralNetModel()
        {
            mlContext = new MLContext();
        }
        private MLContext mlContext { get; set; }

        // Save the model to a file
        public void SaveModel(ITransformer model, string modelPath)
        {
            mlContext.Model.Save(model, null, modelPath);
            Console.WriteLine($"Model saved to: {modelPath}");
        }

        // Load the model from a file
        public ITransformer LoadModel(string modelPath)
        {
            DataViewSchema modelInputSchema;
            ITransformer model = mlContext.Model.Load(modelPath, out modelInputSchema);
            Console.WriteLine($"Model loaded from: {modelPath}");
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
            return model;
        }
        public ITransformer TrainOrRetrainModel(List<NueralNetTrainingDataEntry> data, string modelPath)
        {
            ITransformer model;

            if (File.Exists(modelPath))
            {
                Console.WriteLine("Loading existing model...");
                model = LoadModel(modelPath);
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

            var finalModel = pipeline.Fit(trainData);

            // Save the retrained model
            SaveModel(finalModel, modelPath);

            return finalModel;
        }


        public int PredictAction(ITransformer model, NueralNetworkInput gameState)
        {
            NueralNetTrainingDataEntry trainData = (NueralNetTrainingDataEntry)gameState;
            // Create prediction engine
            var predictionEngine = mlContext.Model.CreatePredictionEngine<NueralNetTrainingDataEntry, NueralNetworkOutput>(model);

            // Predict the action for the given game state
            var prediction = predictionEngine.Predict(trainData);
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
            return mlContext.Transforms.Conversion.MapValueToKey("Action") // Convert Action to categorical key
                    .Append(mlContext.Transforms.Conversion.ConvertType("PlayerHandSum", "PlayerHandSum", DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("DealerUpCard", "DealerUpCard", DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("NumberOfAces", "NumberOfAces", DataKind.Single))
                     .Append(mlContext.Transforms.Conversion.ConvertType("CanSplit", "CanSplit", DataKind.Single))
                    .Append(mlContext.Transforms.Conversion.ConvertType("CanDouble", "CanDouble", DataKind.Single))
                    .Append(mlContext.Transforms.Concatenate("Features", "PlayerHandSum", "DealerUpCard", "NumberOfAces", "CanSplit", "CanDouble"))
                    .Append(mlContext.MulticlassClassification.Trainers.SdcaMaximumEntropy("Action", "Features", maximumNumberOfIterations: 5000))
                    .Append(mlContext.Transforms.Conversion.MapKeyToValue("PredictedAction", "PredictedLabel")); // Convert back to int
        }
    }
}
