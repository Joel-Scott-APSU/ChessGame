using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SwissBracketSplit2
{
    public class Program
    {
        static void Main(string[] args)
        {
            // Initialize the multi-league simulator
            MultiLeagueSimulator multiLeague = new MultiLeagueSimulator();

            // Dictionary to track total points across all simulations
            Dictionary<string, int> teamTotalPoints = new Dictionary<string, int>();

            // Run simulations 3 times
            for (int i = 0; i < 3; i++)
            {
                Console.WriteLine($"\nSimulation {i + 1} Results:");

                // Simulate all leagues and get the top 16 teams
                List<Team> top16Teams = multiLeague.SimulateAllLeagues();

                // Reset team points before starting the simulation
                foreach (var team in top16Teams)
                {
                    team.Points = 0; // Reset individual simulation points
                }

                // Run the major bracket simulation
                MajorBracket majorBracket = new MajorBracket(top16Teams);
                majorBracket.runMajorSim();

                // Accumulate points for each team
                foreach (var team in top16Teams)
                {
                    if (!teamTotalPoints.ContainsKey(team.Name))
                    {
                        teamTotalPoints[team.Name] = 0;
                    }
                    teamTotalPoints[team.Name] += team.Points; // Add points from this simulation
                }
            }

            // Filter out teams with 0 points and sort by total points (descending order)
            var sortedTeams = teamTotalPoints
                .Where(kvp => kvp.Value > 0) // Filter out teams with 0 points
                .OrderByDescending(kvp => kvp.Value) // Sort by total points (descending)
                .Take(16) // Take the top 16 teams
                .ToList();

            // Create a list of top 16 teams
            List<Team> finalTop16 = sortedTeams
                .Select(kvp =>
                {
                    // Find the original team object using GetTeamByName
                    var originalTeam = multiLeague.GetTeamByName(kvp.Key);
                    if (originalTeam != null)
                    {
                        // Directly set the total points on the existing team
                        originalTeam.Points = kvp.Value;
                        return originalTeam; // Add the existing team
                    }
                    return null; // If the team doesn't exist, handle this case accordingly
                })
                .Where(team => team != null) // Ensure you filter out any null teams
                .ToList();



            // Display the top 16 teams
            Console.WriteLine("\nTop 16 Teams:");
            for (int i = 0; i < finalTop16.Count; i++)
            {
                var team = finalTop16[i];
                Console.WriteLine($"{i + 1}. {team.Name} - Total Points: {team.Points}");
            }

            // Proceed with the final tournament simulation
            MajorSimLite majorSimLite = new MajorSimLite(finalTop16);
            List<Team> top8Worlds = majorSimLite.RunGroupStage();
            Tournament worlds = new Tournament();
            worlds.RunTournament(top8Worlds);
        }
    }
}
