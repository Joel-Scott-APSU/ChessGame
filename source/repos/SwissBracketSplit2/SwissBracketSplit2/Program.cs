using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracketSplit2
{
    public class Program
    {
        static void Main(string[] args)
        {
            Split1Sim split1Sim = new Split1Sim();
            Split2Sim split2Sim = new Split2Sim();
            MajorBracket majorBracket = new MajorBracket();

            //split1Sim.runSplit1Sim();
            //split2Sim.runSplit2Sim();
            majorBracket.runMajorSim();
        }

    }
}
