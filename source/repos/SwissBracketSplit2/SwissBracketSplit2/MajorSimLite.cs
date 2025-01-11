using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissBracketSplit2
{
    public class MajorSimLite
    {
        private List<Team> teams;

        public MajorSimLite(List<Team> teams)
        {
            if (teams.Count != 16)
            {
                throw new Exception("Major simulation requires exactly 16 teams.");
            }
            this.teams = teams;
        }

        public List<Team> RunGroupStage()
        {
            Console.WriteLine("Running Group Stage Simulation...");
            RandomProbability random = new RandomProbability();
            Random rng = new Random();

            // Reset wins and losses for all teams before starting
            foreach (var team in teams)
            {
                team.Wins = 0;
                team.Losses = 0;
            }

            int rounds = 0;
            // Simulate Swiss rounds until we have 8 teams remaining (those with less than 3 losses)
            int maxRounds = 5;
            while (rounds < maxRounds)
            {
                rounds++;

                // Exclude teams with 3 losses and 3 wins or more
                var groupedTeams = teams
                    .Where(team => team.Losses < 3 && team.Wins < 3) // Only include teams with fewer than 3 losses and fewer than 3 wins
                    .GroupBy(team => new { team.Wins, team.Losses })
                    .OrderBy(group => group.Key.Wins) // Group first by wins
                    .ToList();

                Console.WriteLine($"\nRound {rounds}");

                foreach (var group in groupedTeams)
                {
                    var groupedList = group.ToList();

                    // Shuffle the grouped list to create random matchups
                    groupedList = groupedList.OrderBy(_ => Guid.NewGuid()).ToList();

                    for (int i = 0; i < groupedList.Count - 1; i += 2)
                    {
                        var team1 = groupedList[i];
                        var team2 = groupedList[i + 1];

                        Console.Write($"{team1.Name} ({team1.Wins}-{team1.Losses}) vs {team2.Name} ({team2.Wins}-{team2.Losses})");
                        string matchResult = random.winProbability(team1, team2);
                        if (matchResult.Contains(team1.Name))
                        {
                            team1.Wins++;
                            team2.Losses++;
                        }
                        else
                        {
                            team2.Wins++;
                            team1.Losses++;
                        }

                        Console.WriteLine($"- Winner: {matchResult}");
                    }
                }
            }

            // Sort teams by Wins (descending), Losses (ascending), and Elo (descending)
            // Here we include all teams to ensure teams with 3 wins are considered.
            List<Team> sortedTeams = teams
                .Where(team => team.Losses < 3) // Only include teams with less than 3 losses
                .OrderByDescending(team => team.Wins)
                .ThenBy(team => team.Losses)
                .ThenByDescending(team => team.EloRating) // Sort by Elo within same records
                .ToList();

            // Take the top 8 teams
            List<Team> winnersGroup = sortedTeams.Take(8).ToList();

            Console.WriteLine("\nTop 8 Teams from Group Stage:");
            foreach (var team in winnersGroup)
            {
                Console.WriteLine($"{team.Name} - Wins: {team.Wins}, Losses: {team.Losses}, Elo: {team.EloRating}");
            }

            return winnersGroup;
        }



    }
}
