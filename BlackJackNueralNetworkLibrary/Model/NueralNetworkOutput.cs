using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackNueralNetworkLibrary.Model
{
    public class NueralNetworkOutput
    {
        [ColumnName("PredictedAction")]
        public int PredictedAction { get; set; }

        [ColumnName("Score")]
        public float[] ActionScores { get; set; }
    }
}
