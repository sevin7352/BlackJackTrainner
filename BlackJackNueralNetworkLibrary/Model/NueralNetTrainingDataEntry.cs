using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackNueralNetworkLibrary.Model
{
    public class NueralNetTrainingDataEntry:NueralNetworkInput
    {
        [ColumnName("Action")] // Ensures correct mapping
        public int Action { get; set; }
    }
}
