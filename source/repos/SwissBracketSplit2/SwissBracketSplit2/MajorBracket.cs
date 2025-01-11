using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissBracketSplit2
{
    public class MajorBracket
    {
        private List<Team> _teams;

        public MajorBracket(List<Team> teams)
        {
            _teams = teams;
        }

        public void runMajorSim()
        {
            Random rand = new Random();
            int round = 1;

            foreach (var team in _teams)
            {
                team.Wins = 0;
                team.Losses = 0;
            }

            List<Team> winnersGroup = new List<Team>();
            List<Team> losersGroup = new List<Team>();

            while (_teams.Count > 0)
            {
                // Pair up teams with the same record
                _teams = _teams.OrderBy(t => t.Wins).ThenBy(t => t.Losses).ToList();
                List<Match> matches = InitializeMatches(_teams);

                // Shuffle matches for randomness
                matches = matches.OrderBy(m => rand.Next()).ToList();

                // Simulate all matches in the current round
                foreach (var match in matches)
                {
                    SimulateMatch(match, rand);
                }

                // Remove teams with 3 wins or 3 losses
                var teamsToRemove = _teams.Where(t => t.Wins >= 3 || t.Losses >= 3).ToList();
                foreach (var team in teamsToRemove)
                {
                    if (team.Wins >= 3)
                    {
                        winnersGroup.Add(team);
                    }
                    else if (team.Losses >= 3)
                    {
                        // Assign points based on the number of wins before elimination
                        switch (team.Wins)
                        {
                            case 0:
                                team.AddPoints(3); // 0 wins, 3 points
                                break;
                            case 1:
                                team.AddPoints(4); // 1 win, 4 points
                                break;
                            case 2:
                                team.AddPoints(5); // 2 wins, 5 points
                                break;
                        }

                        losersGroup.Add(team);
                    }

                    _teams.Remove(team);
                }

                round++;
            }

            // Output the results
            OutputResults(winnersGroup, losersGroup);

            // Simulate the knockout stage
            SimulateKnockoutStage(winnersGroup, rand);
        }


        // Simulate a match between two teams, and return the winning team
        private void SimulateMatch(Match match, Random rand)
        {
            double winProbabilityTeam1 = match.Team1.GetAdjustedWinProbability(match.Team2);
            double number = rand.NextDouble();

            if (number < winProbabilityTeam1)
            {
                match.Winner = match.Team1;
                match.Loser = match.Team2;
                match.Team1.Wins++;
                match.Team2.Losses++;
            }
            else
            {
                match.Winner = match.Team2;
                match.Loser = match.Team1;
                match.Team2.Wins++;
                match.Team1.Losses++;
            }

            match.IsMatchPlayed = true;
        }

        private void OutputResults(List<Team> winnersGroup, List<Team> losersGroup)
        {
            Console.WriteLine();
            Console.WriteLine("Final Standings:");
            Console.WriteLine("===================\n");

            // Sort losersGroup by number of wins and then alphabetically
            var groupedLosers = losersGroup.OrderBy(t => t.Wins).ThenBy(t => t.Name);

            Console.WriteLine("Eliminated:");
            Console.WriteLine("---------------------------");

            int currentWins = -1; // Start with a value that won't match any real wins count

            foreach (var team in groupedLosers)
            {
                if (team.Wins == 1)
                {
                    // Do not add points here, just output the standings
                    Console.WriteLine($"{team.Name} - Wins: {team.Wins}, Losses: {team.Losses}");
                }
                else if (team.Wins == 2)
                {
                    // Same, just output the standings
                    Console.WriteLine($"{team.Name} - Wins: {team.Wins}, Losses: {team.Losses}");
                }
                else
                {
                    // Same, output the standings
                    Console.WriteLine($"{team.Name} - Wins: {team.Wins}, Losses: {team.Losses}");
                }
                Console.WriteLine("---------------------------");
            }

            Console.WriteLine();
            Console.WriteLine("\nQuarterfinals (QF - 6 Points):");
            Console.WriteLine("---------------------------");
        }


        private void SimulateKnockoutStage(List<Team> winnersGroup, Random rand)
        {
            // Ensure winnersGroup has exactly 8 teams for the knockout stage
            if (winnersGroup.Count != 8)
            {
                winnersGroup = winnersGroup.OrderByDescending(t => t.Wins).ThenByDescending(t => t.Losses).Take(8).ToList();
            }

            // Quarterfinals
            List<Match> quarterfinals = InitializeMatches(winnersGroup);
            Console.WriteLine("\nQuarterfinals (QF - 6 Points):");
            Console.WriteLine("---------------------------");

            // Simulate quarterfinal matches
            foreach (var match in quarterfinals)
            {
                SimulateMatch(match, rand);
            }

            // Sort quarterfinals by loser's Name alphabetically
            var sortedQuarterfinals = quarterfinals.OrderBy(m => m.Loser != null ? m.Loser.Name : "");

            // Print quarterfinal results alphabetically by loser
            foreach (var match in sortedQuarterfinals)
            {
                Console.WriteLine($"QF: {match.Team1.Name} vs {match.Team2.Name} - Loser: {(match.Loser != null ? match.Loser.Name : "Not determined yet")}");
                Console.WriteLine("---------------------------");
                Console.WriteLine();
                match.Loser.AddPoints(6); // Award points only for the loser after they are eliminated
            }

            List<Team> semifinalists = quarterfinals.Select(m => m.Winner).ToList();

            // Semifinals
            Console.WriteLine("\nSemifinals (SF - 9 Points):");
            Console.WriteLine("---------------------------");

            // Initialize semifinals based on quarterfinal winners
            List<Match> semifinals = InitializeMatches(semifinalists);

            // Simulate semifinal matches
            foreach (var match in semifinals)
            {
                SimulateMatch(match, rand);
            }

            // Sort semifinals by loser's Name alphabetically
            var sortedSemifinals = semifinals.OrderBy(m => m.Loser != null ? m.Loser.Name : "");

            // Print semifinal results alphabetically by loser
            foreach (var match in sortedSemifinals)
            {
                Console.WriteLine($"SF: {match.Team1.Name} vs {match.Team2.Name} - Loser: {(match.Loser != null ? match.Loser.Name : "Not determined yet")}");
                Console.WriteLine("---------------------------");
                Console.WriteLine();
                match.Loser.AddPoints(9); // Award points only for the loser after they are eliminated
            }

            List<Team> finalists = semifinals.Select(m => m.Winner).ToList();

            // Final
            Console.WriteLine("\nFinal (winner - 16 Points | Runner-Up - 12 Points):");
            Console.WriteLine("---------------------------");
            Match final = new Match { Team1 = finalists[0], Team2 = finalists[1] };
            SimulateMatch(final, rand);
            Console.WriteLine($"Final: {final.Team1.Name} vs {final.Team2.Name}");
            Console.WriteLine("---------------------------");
            Console.WriteLine($"Winner: {final.Winner.Name}");
            Console.WriteLine();

            // Display the runner-up
            Team runnerUp = finalists.First(team => team != final.Winner);
            Console.WriteLine($"Runner-Up: {runnerUp.Name}");
            Console.WriteLine();
            runnerUp.AddPoints(12); // Award points only to the runner-up when they are determined
            final.Winner.AddPoints(16); // Award points only to the winner when they are determined
        }


        // Helper method to initialize matches
        private List<Match> InitializeMatches(List<Team> teams)
        {
            List<Match> matches = new List<Match>();
            for (int i = 0; i < teams.Count; i += 2)
            {
                matches.Add(new Match { Team1 = teams[i], Team2 = teams[i + 1] });
            }
            return matches;
        }
    }
}