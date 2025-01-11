using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracketSplit2
{
    public class RandomProbability
    {
        public RandomProbability() { }
        public string winProbability(Team team1, Team team2)
        {

            double team1WinProbability = team1.GetAdjustedWinProbability(team2);
            double team2WinProbability = 1 - team1WinProbability;

            Random rng = new Random();
            double randomValue = rng.NextDouble(); // Generates a number between 0.0 and 1.0

            if (randomValue <= team2WinProbability)
            {
                return team2.Name;
            }
            else
            {
                return team1.Name;
            }
        }
    }
}
