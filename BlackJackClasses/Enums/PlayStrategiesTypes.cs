using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Enums
{
    public enum PlayStrategiesTypes{
        Random = 0,
        SingleHandBook =1,
        SingleHandAdaptive = 2,
        MachineLearning = 3,
    }

    public static class PlayStrategies
    {
        public static Dictionary<PlayStrategiesTypes,string> AvailablePlayStrategies { get; set; } = new Dictionary<PlayStrategiesTypes, string>()
        {
            {PlayStrategiesTypes.Random, "Random" },
            {PlayStrategiesTypes.SingleHandBook, "Single Hand book" },
            {PlayStrategiesTypes.SingleHandAdaptive,"Single Hand Adaptive" },
            {PlayStrategiesTypes.MachineLearning,"MachineLearning" }
        };
    }

}
