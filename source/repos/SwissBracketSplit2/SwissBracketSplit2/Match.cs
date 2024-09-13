using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracket
{
    public class Match
    {
            public Team Team1 { get; set; }
            public Team Team2 { get; set; }
            public Team Winner { get; set; }
            public Team Loser { get; set; }
            public bool isMatchPlayed { get; set; }
        }
    }
