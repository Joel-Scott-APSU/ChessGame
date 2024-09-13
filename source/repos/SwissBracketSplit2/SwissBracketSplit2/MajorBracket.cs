using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracket
{
    public class MajorBracket
    {
        public MajorBracket() { }
            public void runMajorSim()
            {
                List<Team> teams = InitializeTeams();
                List<Match> matches = new List<Match>();

                // Lists to keep track of teams with 3 wins or 3 losses
                List<Team> winnersGroup = new List<Team>();
                List<Team> losersGroup = new List<Team>();

                // Simulate tournament rounds
                SimulateTournament(teams, winnersGroup, losersGroup);

                // Output final results and knockout stage
                OutputResults(winnersGroup, losersGroup);
                SimulateKnockoutStage(winnersGroup);

                // Wait for user input before closing
                Console.WriteLine("Press Enter to close...");
                Console.ReadLine();
            }

            static List<Team> InitializeTeams()
            {
                List<Team> teams = new List<Team>
            {


                // Major Group
new Team("Inter Miami"),
new Team("Adidas All Stars"),
new Team("Villarreal CF"),
new Team("New England"),
new Team("Barcelona"),
new Team("Bergamo Calcio"),
new Team("Spurs"),
new Team("Arsenal"),
new Team("France"),
new Team("Bayern Munich"),
new Team("Napoli"),
new Team("Atletico"),
new Team("Real Sociedad"),
new Team("Manchester United"),
new Team("Inter"),
new Team("Liverpool"),



            };



                return teams;
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

            static void SimulateTournament(List<Team> teams, List<Team> winnersGroup, List<Team> losersGroup)
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

                match.isMatchPlayed = true;
            }



            static void OutputResults(List<Team> winnersGroup, List<Team> losersGroup)
            {
                Console.WriteLine();
                Console.WriteLine("Final Standings:");
                Console.WriteLine("===================\n");

                // Sort losersGroup by number of wins and then alphabetically
                var groupedLosers = losersGroup.OrderBy(t => t.Wins).ThenBy(t => t.name);

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

                    Console.WriteLine($"{team.name} - Wins: {team.Wins}, Losses: {team.Losses}");
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
                }

                // Sort quarterfinals by loser's name alphabetically
                var sortedQuarterfinals = quarterfinals.OrderBy(m => m.Loser != null ? m.Loser.name : "");

                // Print quarterfinal results alphabetically by loser
                foreach (var match in sortedQuarterfinals)
                {
                    Console.WriteLine($"QF: {match.Team1.name} vs {match.Team2.name} - Loser: {(match.Loser != null ? match.Loser.name : "Not determined yet")}");
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
                }

                // Sort semifinals by loser's name alphabetically
                var sortedSemifinals = semifinals.OrderBy(m => m.Loser != null ? m.Loser.name : "");

                // Print semifinal results alphabetically by loser
                foreach (var match in sortedSemifinals)
                {
                    Console.WriteLine($"SF: {match.Team1.name} vs {match.Team2.name} - Loser: {(match.Loser != null ? match.Loser.name : "Not determined yet")}");
                    Console.WriteLine("---------------------------");
                    Console.WriteLine();
                }

                List<Team> finalists = semifinals.Select(m => m.Winner).ToList();

                // Final
                Console.WriteLine("\nFinal (winner - 16 Points | Runner-Up - 12 Points):");
                Console.WriteLine("---------------------------");
                Match final = new Match { Team1 = finalists[0], Team2 = finalists[1] };
                SimulateMatch(final, rand);
                Console.WriteLine($"Final: {final.Team1.name} vs {final.Team2.name}");
                Console.WriteLine("---------------------------");
                Console.WriteLine($"Winner: {final.Winner.name}");
                Console.WriteLine();

                // Display the runner-up
                Team runnerUp = finalists.First(team => team != final.Winner);
                Console.WriteLine($"Runner-Up: {runnerUp.name}");
                Console.WriteLine();
            }

        }

    }
