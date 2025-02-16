using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Enums;

namespace BlackJackTrainner.Model
{
   public class HandSuggestions
    {

        public HandSuggestions()
        {
            Stay = 0;
            Hit = 0;
            Split = 0;
            DoubleDown = 0;
        }

        
        public double Stay { get; set; }
        public double Hit { get; set; }
        public double Split { get; set; }
        public double DoubleDown { get; set; }

        public ActionTypes SuggestedAction(bool canSplit,bool canDouble,bool canHit)
        {
            if ((DoubleDown >= Stay && DoubleDown >= Hit && (DoubleDown >= Split || !canSplit)) && canDouble)
            {
                return ActionTypes.Double;
            }
            if (canSplit && Split >= DoubleDown && Split >= Hit && Split >= Stay)
            {
                return ActionTypes.Split;
            }
            if (((Hit >= DoubleDown || !canDouble) && (Hit >= Stay || !canSplit) && Hit >= Stay) && canHit)
            {
                return ActionTypes.Hit;
            }
            if ((Stay >= DoubleDown || !canDouble) && (Stay >= Split || !canSplit) && Stay >= Hit)
            {
                return ActionTypes.Stay;
            }

            return ActionTypes.Stay;

        }

    }
}
