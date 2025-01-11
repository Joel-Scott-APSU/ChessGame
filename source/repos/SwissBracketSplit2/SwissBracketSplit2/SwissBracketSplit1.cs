using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissBracket
{
    class Split1Sim
    {
        public Split1Sim() { }
        public void runSplit1Sim()
        {
            List<Team> group1Teams = InitializeTeamsGroup1();
            List<Team> group2Teams = InitializeTeamsGroup2();
            List<Team> winnersGroup = new List<Team>();
            List<Team> losersGroup = new List<Team>();

            int totalSimulations = 3; // Number of simulations to run

            for (int i = 0; i < totalSimulations; i++)
            {
                Console.WriteLine($"\nSimulation {i + 1}:\n");

                // Run the tourNament simulation
                SimulateTourNament(group1Teams, winnersGroup, losersGroup);

                // Output results for the current simulation
                OutputResults(winnersGroup, losersGroup);
                SimulateKnockoutStage(winnersGroup);
                // Reset teams for the next simulation
                ResetTeams(group1Teams);

                // Clear the winners and losers groups for the next simulation
                winnersGroup.Clear();
                losersGroup.Clear();
            }

            // Output the final cumulative points after all simulations
            Console.WriteLine("\nFinal Cumulative Points After 3 Simulations for Group 1:");
            Console.WriteLine("=============================================");
            foreach (var team in group1Teams.OrderBy(t => t.Name))
            {
                Console.WriteLine($"{team.Name} - Total Points: {team.Points}");
            }

            for (int i = 0; i < totalSimulations; i++)
            {
                Console.WriteLine($"\nSimulation {i + 1}:\n");

                SimulateTourNament(group2Teams, winnersGroup, losersGroup);

                OutputResults(winnersGroup, losersGroup);
                SimulateKnockoutStage(winnersGroup);

                ResetTeams(group2Teams);

                winnersGroup.Clear();
                losersGroup.Clear();
            }

            // Output the final cumulative points after all simulations
            Console.WriteLine("\nFinal Cumulative Points After 3 Simulations for Group 2:");
            Console.WriteLine("=============================================");
            foreach (var team in group2Teams.OrderBy(t => t.Name))
            {
                Console.WriteLine($"{team.Name} - Total Points: {team.Points}");
            }

            // Wait for user input before closing
            Console.WriteLine("Press Enter to close...");
            Console.ReadLine();
        }

        static void ResetTeams(List<Team> teams)
        {
            foreach (var team in teams)
            {
                team.Wins = 0;
                team.Losses = 0;
            }
        }



        static List<Team> InitializeTeamsGroup1()
        {
            List<Team> teams = new List<Team>
            {
        // Group 1
        
                new Team("Brazil", 1975),
                new Team("Italy", 1950),
                new Team("Inter Miami", 2300),   // High Elo based on your preferences
                new Team("Real Madrid", 1985),
                new Team("Bergamo Calcio", 1935),
                new Team("Spain", 1950),
                new Team("Liverpool", 1930),
                new Team("Atletico", 1925),
                new Team("Manchester United", 1940),
                new Team("Soccer Aid", 2300),    // Highest Elo based on your preferences
                new Team("Dortmund", 1935),
                new Team("England", 1960),
                new Team("RB Leipzig", 1880),
                new Team("Spurs", 1915),
                new Team("Bayern Munich", 1950),
                new Team("Manchester City", 1945)
            };

            return teams;
        }

        static List<Team> InitializeTeamsGroup2()
        {
            List<Team> group2 = new List<Team>
            {
                new Team("Adidas All Stars", 2240),   // High Elo based on your preferences
                new Team("Inter", 1900),
                new Team("Villarreal CF", 1890),
                new Team("Belgium", 1945),
                new Team("Portugal", 1975),
                new Team("France", 1965),
                new Team("Napoli", 1925),
                new Team("Real Sociedad", 1910),
                new Team("PSG", 1940),
                new Team("Germany", 1960),
                new Team("Argentina", 1960),
                new Team("Chelsea", 1930),
                new Team("Barcelona", 1950),
                new Team("Arsenal", 1915),
                new Team("New England", 2400),   // Highest Elo based on your preferences
                new Team("Leicester City", 1875)
            };

            return group2;
        }


        static List<Match> InitializeMatches(List<Team> teams)
        {
            List<Match> matches = new List<Match>();

            // Pair up teams with the same record
            for (int i = 0; i < teams.Count; i += 2)
            {
                matches.Add(new Match { Team1 = teams[i], Team2 = teams[i + 1] });
            }

            return matches;
        }

        static void SimulateTourNament(List<Team> teams, List<Team> winnersGroup, List<Team> losersGroup)
        {
            Random rand = new Random();
            int round = 1;

            while (teams.Count > 0)
            {
                // Pair up teams with the same record
                teams = teams.OrderBy(t => t.Wins).ThenBy(t => t.Losses).ToList();
                List<Match> matches = InitializeMatches(teams);

                // Shuffle matches for randomness
                matches = matches.OrderBy(m => rand.Next()).ToList();

                // Simulate all matches in the current round
                foreach (var match in matches)
                {
                    SimulateMatch(match, rand);
                }

                // Remove teams with 3 wins or 3 losses
                var teamsToRemove = teams.Where(t => t.Wins >= 3 || t.Losses >= 3).ToList();
                foreach (var team in teamsToRemove)
                {
                    if (team.Wins >= 3)
                        winnersGroup.Add(team);
                    else if (team.Losses >= 3)
                        losersGroup.Add(team);

                    teams.Remove(team);
                }

                round++;
            }
        }

        static void SimulateMatch(Match match, Random rand)
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



        static void OutputResults(List<Team> winnersGroup, List<Team> losersGroup)
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
                if (team.Wins > currentWins)
                {
                    currentWins = team.Wins;
                    Console.WriteLine();
                    if (currentWins == 1)
                        Console.WriteLine($"{currentWins} Win");
                    else
                        Console.WriteLine($"{currentWins} Wins");
                    Console.WriteLine("---------------------------");
                }

                if (team.Wins == 1)
                {
                    team.Points += 3;
                }
                else if (team.Wins == 2)
                {
                    team.Points += 4;
                }
                else
                {
                    team.Points += 5;
                }

                Console.WriteLine($"{team.Name} - Wins: {team.Wins}, Losses: {team.Losses}");
                Console.WriteLine("---------------------------");
            }

            Console.WriteLine();

            // Ensure a new line before Quarterfinals
            Console.WriteLine("\nQuarterfinals (QF - 6 Points):");
            Console.WriteLine("---------------------------");
        }

        static void SimulateKnockoutStage(List<Team> winnersGroup)
        {
            Random rand = new Random();

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
                match.Loser.Points += 6;
            }

            // Sort quarterfinals by loser's Name alphabetically
            var sortedQuarterfinals = quarterfinals.OrderBy(m => m.Loser != null ? m.Loser.Name : "");

            // Print quarterfinal results alphabetically by loser
            foreach (var match in sortedQuarterfinals)
            {
                Console.WriteLine($"QF: {match.Team1.Name} vs {match.Team2.Name} - Loser: {(match.Loser != null ? match.Loser.Name : "Not determined yet")}");
                Console.WriteLine("---------------------------");
                Console.WriteLine();
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
                match.Loser.Points += 9;
            }

            // Sort semifinals by loser's Name alphabetically
            var sortedSemifinals = semifinals.OrderBy(m => m.Loser != null ? m.Loser.Name : "");

            // Print semifinal results alphabetically by loser
            foreach (var match in sortedSemifinals)
            {
                Console.WriteLine($"SF: {match.Team1.Name} vs {match.Team2.Name} - Loser: {(match.Loser != null ? match.Loser.Name : "Not determined yet")}");
                Console.WriteLine("---------------------------");
                Console.WriteLine();
            }

            List<Team> finalists = semifinals.Select(m => m.Winner).ToList();

            // Final
            Console.WriteLine("\nFinal (winner - 16 Points | Runner-Up - 12 Points):");
            Console.WriteLine("---------------------------");
            Match final = new Match { Team1 = finalists[0], Team2 = finalists[1] };
            SimulateMatch(final, rand);
            final.Winner.Points += 16;
            Console.WriteLine($"Final: {final.Team1.Name} vs {final.Team2.Name}");
            Console.WriteLine("---------------------------");
            Console.WriteLine($"Winner: {final.Winner.Name}");
            Console.WriteLine();

            // Display the runner-up
            Team runnerUp = finalists.First(team => team != final.Winner);
            runnerUp.Points += 12;
            Console.WriteLine($"Runner-Up: {runnerUp.Name}");
            Console.WriteLine();
        }
    }

}
