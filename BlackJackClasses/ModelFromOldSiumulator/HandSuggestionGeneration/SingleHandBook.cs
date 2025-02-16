using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BlackJackClasses.Model;
using PlayingCards;

namespace BlackJackTrainner.Model.HandSuggestionGeneration
{
    public static class SingleHandBook
    {

        public static HandSuggestions LookUpSuggestions(List<PlayingCard> playersHand, int dealersValue,bool canSplit)
        {
            HandSuggestions handSuggestions = new HandSuggestions();
            if (playersHand.Count == 2 && (canSplit || playersHand.Count(p=>p.isAce)>0))
            {
                int[] playersCards = new int[] {playersHand[0].CardNumber,playersHand[1].CardNumber }; 
                if (playersHand[0].CardNumber == playersHand[1].CardNumber && (playersHand[0].CardNumber == 11 || playersHand[0].CardNumber == 12 || playersHand[0].CardNumber == 13))
                {
                    playersCards = new int[]{10,10};
                }

                var returned = SingleHandBookSpecialEntries.Where(p =>
                    p.DealersUpCard == dealersValue && p.PlayersHand.OrderBy(x=> x).SequenceEqual(playersCards.OrderBy(y=> y))).ToList();
                if (returned != null)
                {
                    if (returned.Count() > 1)
                    {
                        string message = String.Empty;
                        foreach (var entry in returned)
                        {
                            message = message + entry.DealersUpCard + " : " + playersHand.ToString();
                        }

                        MessageBox.Show("Multiple entries returned\n" + message);
                        return new HandSuggestions();
                    }

                    if (returned.Count == 1)
                    {
                        return returned[0].HandSuggestion;
                    }
                }
            }

            if (GameStateExtensions.calculateValue(playersHand) <=8 )
            {
                return new HandSuggestions(){Hit = 100};
            }
            if (GameStateExtensions.calculateValue(playersHand) >= 17)
            {
                //not needed now, all rules added
                return new HandSuggestions(){Stay = 100};
            }
            var returnedEntries = SingleHandBookEntries.Where(p =>
                p.DealersUpCard == dealersValue && p.PlayersHandvalue == GameStateExtensions.calculateValue(playersHand)).ToList();
            if (returnedEntries != null)
            {
                if (returnedEntries.Count() > 1)
                {
                    string message = String.Empty;
                    foreach (var entry in returnedEntries)
                    {
                        message = message + entry.DealersUpCard + " : " + playersHand.ToString();
                    }

                    MessageBox.Show("Multiple entries returned\n" + message);
                    return new HandSuggestions();
                }

                if (returnedEntries.Count == 1)
                {
                    return returnedEntries[0].HandSuggestion;
                }
            }



            return handSuggestions;
        }

        public static SingleHandBookEntry[] SingleHandBookEntries = new SingleHandBookEntry[]
        {
            //2
            new SingleHandBookEntry(2,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(2,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(2,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(2,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(2,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(2,13,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,14,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,15,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,16,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(2,21,new HandSuggestions(){Stay = 100}),

            //3
            new SingleHandBookEntry(3,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(3,9,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(3,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(3,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(3,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(3,13,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,14,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,15,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,16,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(3,21,new HandSuggestions(){Stay = 100}),

            //4
            new SingleHandBookEntry(4,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(4,9,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(4,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(4,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(4,12,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,13,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,14,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,15,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,16,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(4,21,new HandSuggestions(){Stay = 100}),

            //5
            new SingleHandBookEntry(5,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(5,9,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(5,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(5,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(5,12,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,13,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,14,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,15,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,16,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(5,21,new HandSuggestions(){Stay = 100}),

            //6
            new SingleHandBookEntry(6,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(6,9,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(6,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(6,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(6,12,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,13,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,14,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,15,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,16,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(6,21,new HandSuggestions(){Stay = 100}),

            //7
            new SingleHandBookEntry(7,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(7,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(7,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,13,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,14,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,15,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,16,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(7,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(7,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(7,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(7,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(7,21,new HandSuggestions(){Stay = 100}),

            //8
            new SingleHandBookEntry(8,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(8,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(8,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,13,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,14,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,15,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,16,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(8,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(8,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(8,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(8,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(8,21,new HandSuggestions(){Stay = 100}),
            
            //9
            new SingleHandBookEntry(9,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,10,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(9,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(9,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,13,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,14,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,15,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,16,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(9,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(9,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(9,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(9,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(9,21,new HandSuggestions(){Stay = 100}),

            //10
            new SingleHandBookEntry(10,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,10,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,11,new HandSuggestions(){DoubleDown = 100}),
            new SingleHandBookEntry(10,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,13,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,14,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,15,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,16,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(10,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(10,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(10,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(10,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(10,21,new HandSuggestions(){Stay = 100}),

            //11
            new SingleHandBookEntry(11,8,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,9,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,10,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,11,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,12,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,13,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,14,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,15,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,16,new HandSuggestions(){Hit = 100}),
            new SingleHandBookEntry(11,17,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(11,18,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(11,19,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(11,20,new HandSuggestions(){Stay = 100}),
            new SingleHandBookEntry(11,21,new HandSuggestions(){Stay = 100}),

        };

        public static SingleHandBookEntry[] SingleHandBookSpecialEntries = new SingleHandBookEntry[]
        {
            //2
            
            new SingleHandBookEntry(2,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(2,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(2,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(2,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(2,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(2,18,new HandSuggestions(){Stay = 100},new int[]{1,7}),
            new SingleHandBookEntry(2,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(2,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(2,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(2,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(2,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(2,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(2,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(2,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(2,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(2,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(2,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(2,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(2,12,new HandSuggestions(){Split = 100},new int[]{1,1}),
            

            //3

            new SingleHandBookEntry(3,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(3,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(3,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(3,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(3,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(3,18,new HandSuggestions(){Stay = 25,DoubleDown = 75},new int[]{1,7}),
            new SingleHandBookEntry(3,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(3,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(3,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(3,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(3,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(3,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(3,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(3,12,new HandSuggestions(){Split = 100},new int[]{6,6}),
            new SingleHandBookEntry(3,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(3,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(3,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(3,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(3,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //4
            new SingleHandBookEntry(4,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(4,12,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(4,14,new HandSuggestions(){DoubleDown = 100},new int[]{1,4}),
            new SingleHandBookEntry(4,15,new HandSuggestions(){DoubleDown = 100},new int[]{1,5}),
            new SingleHandBookEntry(4,16,new HandSuggestions(){DoubleDown = 100},new int[]{1,6}),
            new SingleHandBookEntry(4,17,new HandSuggestions(){Stay = 25,DoubleDown = 75},new int[]{1,7}),
            new SingleHandBookEntry(4,18,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(4,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(4,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(4,4,new HandSuggestions(){Split = 100},new int[]{2,2}),
            new SingleHandBookEntry(4,6,new HandSuggestions(){Split = 100},new int[]{3,3}),
            new SingleHandBookEntry(4,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(4,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(4,12,new HandSuggestions(){Split = 100},new int[]{6,6}),
            new SingleHandBookEntry(4,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(4,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(4,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(4,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(4,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //5
            new SingleHandBookEntry(5,13,new HandSuggestions(){DoubleDown = 100},new int[]{1,2}),
            new SingleHandBookEntry(5,14,new HandSuggestions(){DoubleDown = 100},new int[]{1,3}),
            new SingleHandBookEntry(5,15,new HandSuggestions(){DoubleDown = 100},new int[]{1,4}),
            new SingleHandBookEntry(5,16,new HandSuggestions(){DoubleDown = 100},new int[]{1,5}),
            new SingleHandBookEntry(5,17,new HandSuggestions(){DoubleDown = 100},new int[]{1,6}),
            new SingleHandBookEntry(5,18,new HandSuggestions(){Stay = 25,DoubleDown = 75},new int[]{1,7}),
            new SingleHandBookEntry(5,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(5,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(5,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(5,4,new HandSuggestions(){Split = 100},new int[]{2,2}),
            new SingleHandBookEntry(5,6,new HandSuggestions(){Split = 100},new int[]{3,3}),
            new SingleHandBookEntry(5,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(5,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(5,12,new HandSuggestions(){Split = 100},new int[]{6,6}),
            new SingleHandBookEntry(5,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(5,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(5,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(5,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(5,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //6
            new SingleHandBookEntry(6,13,new HandSuggestions(){DoubleDown = 100},new int[]{1,2}),
            new SingleHandBookEntry(6,14,new HandSuggestions(){DoubleDown = 100},new int[]{1,3}),
            new SingleHandBookEntry(6,15,new HandSuggestions(){DoubleDown = 100},new int[]{1,4}),
            new SingleHandBookEntry(6,16,new HandSuggestions(){DoubleDown = 100},new int[]{1,5}),
            new SingleHandBookEntry(6,17,new HandSuggestions(){DoubleDown = 100},new int[]{1,6}),
            new SingleHandBookEntry(6,18,new HandSuggestions(){Stay = 25,DoubleDown = 75},new int[]{1,7}),
            new SingleHandBookEntry(6,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(6,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(6,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(6,4,new HandSuggestions(){Split = 100},new int[]{2,2}),
            new SingleHandBookEntry(6,6,new HandSuggestions(){Split = 100},new int[]{3,3}),
            new SingleHandBookEntry(6,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(6,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(6,12,new HandSuggestions(){Split = 100},new int[]{6,6}),
            new SingleHandBookEntry(6,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(6,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(6,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(6,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(6,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //7
            new SingleHandBookEntry(7,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(7,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(7,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(7,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(7,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(7,18,new HandSuggestions(){Stay = 100},new int[]{1,7}),
            new SingleHandBookEntry(7,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(7,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(7,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(7,4,new HandSuggestions(){Split = 100},new int[]{2,2}),
            new SingleHandBookEntry(7,6,new HandSuggestions(){Split = 100},new int[]{3,3}),
            new SingleHandBookEntry(7,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(7,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(7,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(7,14,new HandSuggestions(){Split = 100},new int[]{7,7}),
            new SingleHandBookEntry(7,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(7,18,new HandSuggestions(){Stay = 100},new int[]{9,9}),
            new SingleHandBookEntry(7,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(7,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //8
            new SingleHandBookEntry(8,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(8,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(8,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(8,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(8,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(8,18,new HandSuggestions(){Stay = 100},new int[]{1,7}),
            new SingleHandBookEntry(8,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(8,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(8,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(8,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(8,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(8,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(8,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(8,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(8,14,new HandSuggestions(){Hit = 100},new int[]{7,7}),
            new SingleHandBookEntry(8,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(8,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(8,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(8,12,new HandSuggestions(){Split = 100},new int[]{1,1}),
            
            //9
            new SingleHandBookEntry(9,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(9,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(9,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(9,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(9,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(9,18,new HandSuggestions(){Hit = 100},new int[]{1,7}),
            new SingleHandBookEntry(9,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(9,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(9,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(9,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(9,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(9,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(9,10,new HandSuggestions(){DoubleDown = 100},new int[]{5,5}),
            new SingleHandBookEntry(9,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(9,14,new HandSuggestions(){Hit = 100},new int[]{7,7}),
            new SingleHandBookEntry(9,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(9,18,new HandSuggestions(){Split = 100},new int[]{9,9}),
            new SingleHandBookEntry(9,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(9,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //10
            new SingleHandBookEntry(10,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(10,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(10,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(10,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(10,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(10,18,new HandSuggestions(){Hit = 100},new int[]{1,7}),
            new SingleHandBookEntry(10,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(10,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(10,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(10,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(10,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(10,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(10,10,new HandSuggestions(){Hit = 100},new int[]{5,5}),
            new SingleHandBookEntry(10,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(10,14,new HandSuggestions(){Hit = 100},new int[]{7,7}),
            new SingleHandBookEntry(10,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(10,18,new HandSuggestions(){Stay = 100},new int[]{9,9}),
            new SingleHandBookEntry(10,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(10,12,new HandSuggestions(){Split = 100},new int[]{1,1}),

            //11
            new SingleHandBookEntry(11,13,new HandSuggestions(){Hit = 100},new int[]{1,2}),
            new SingleHandBookEntry(11,14,new HandSuggestions(){Hit = 100},new int[]{1,3}),
            new SingleHandBookEntry(11,15,new HandSuggestions(){Hit = 100},new int[]{1,4}),
            new SingleHandBookEntry(11,16,new HandSuggestions(){Hit = 100},new int[]{1,5}),
            new SingleHandBookEntry(11,17,new HandSuggestions(){Hit = 100},new int[]{1,6}),
            new SingleHandBookEntry(11,18,new HandSuggestions(){Hit = 100},new int[]{1,7}),
            new SingleHandBookEntry(11,19,new HandSuggestions(){Stay = 100},new int[]{1,8}),
            new SingleHandBookEntry(11,20,new HandSuggestions(){Stay = 100},new int[]{1,9}),
            new SingleHandBookEntry(11,21,new HandSuggestions(){Stay = 100},new int[]{1,10}),//BlackJack

            new SingleHandBookEntry(11,4,new HandSuggestions(){Hit = 100},new int[]{2,2}),
            new SingleHandBookEntry(11,6,new HandSuggestions(){Hit = 100},new int[]{3,3}),
            new SingleHandBookEntry(11,8,new HandSuggestions(){Hit = 100},new int[]{4,4}),
            new SingleHandBookEntry(11,10,new HandSuggestions(){Hit = 100},new int[]{5,5}),
            new SingleHandBookEntry(11,12,new HandSuggestions(){Hit = 100},new int[]{6,6}),
            new SingleHandBookEntry(11,14,new HandSuggestions(){Hit = 100},new int[]{7,7}),
            new SingleHandBookEntry(11,16,new HandSuggestions(){Split = 100},new int[]{8,8}),
            new SingleHandBookEntry(11,18,new HandSuggestions(){Stay = 100},new int[]{9,9}),
            new SingleHandBookEntry(11,20,new HandSuggestions(){Stay = 100},new int[]{10,10}),
            new SingleHandBookEntry(11,12,new HandSuggestions(){Split = 100},new int[]{1,1}),


        };



    }
}
