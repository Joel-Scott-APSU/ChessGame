using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracket
{
    public class Team
    {
            public string name { get; set; }
            public int Wins { get; set; }
            public int Losses { get; set; }

            public int Points { get; set; }
            public Team(string name)
            {
                this.Points = 0;
                this.name = name;
                this.Wins = 0;
                this.Losses = 0;
            }

        public double GetAdjustedWinProbability(Team opponent)
        {
            if (name == "New England" || name == "Inter Miami")
            {
                if (opponent.name == "Inter Miami" || opponent.name == "New England")
                {
                    return 0.5;
                }
                else if (opponent.name == "Soccer Aid" || opponent.name == "Adidas All Stars")
                {
                    return 0.6;
                }
                else
                {
                    return 0.8;
                }
            }
            else if (name == "Soccer Aid" || name == "Adidas All Stars")
            {
                if (opponent.name == "New England" || opponent.name == "Inter Miami")
                {
                    return 0.4;
                }
                else if (opponent.name == "Soccer Aid" || opponent.name == "Adidas All Stars")
                {
                    return 0.5;
                }
                else
                {
                    return 0.75;
                }
            }
            else
            {
                if (opponent.name == "New England" || opponent.name == "Inter Miami")
                {
                    return 0.2;
                }
                else if (opponent.name == "Adidas All Stars" || opponent.name == "Soccer Aid")
                {
                    return 0.3;
                }
                return 0.5;
            }
        }

    }
}
