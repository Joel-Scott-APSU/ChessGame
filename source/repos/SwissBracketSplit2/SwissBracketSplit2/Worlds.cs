using SwissBracket;
using System;
using System.Collections.Generic;

namespace SwissBracketSplit2
{
    public class Tournament
    {
        public Tournament() { }

        // Method to simulate and display the match results
        public void RunTournament(List<Team> rankedTeams)
        {
            if (rankedTeams.Count != 8)
            {
                throw new Exception("You must have exactly 8 teams.");
            }

            // Seeded teams based on ranking
            Team firstSeed = rankedTeams[0];
            Team secondSeed = rankedTeams[1];
            Team thirdSeed = rankedTeams[2];
            Team fourthSeed = rankedTeams[3];
            Team fifthSeed = rankedTeams[4];
            Team sixthSeed = rankedTeams[5];
            Team seventhSeed = rankedTeams[6];
            Team eighthSeed = rankedTeams[7];

            RandomProbability random = new RandomProbability();

            // 3rd Seed Match
            Console.WriteLine("3rd Seed Match");
            Console.WriteLine("---------------------");
            Console.WriteLine($"{thirdSeed.Name} vs {fourthSeed.Name}");
            string thirdSeedMatchResult = random.winProbability(thirdSeed, fourthSeed);
            Team winnerOf3rdSeedMatch = thirdSeedMatchResult.Contains(thirdSeed.Name) ? thirdSeed : fourthSeed;
            Team loserOf3rdSeedMatch = thirdSeedMatchResult.Contains(thirdSeed.Name) ? fourthSeed : thirdSeed; // Get the loser of the match
            Console.WriteLine(thirdSeedMatchResult);
            Console.WriteLine();

            // 4th/5th Seed Match (Loser of the 3rd Seed Match vs 5th Seed)
            Console.WriteLine("4th/5th Seed Match");
            Console.WriteLine("---------------------");
            Console.WriteLine($"{loserOf3rdSeedMatch.Name} vs {fifthSeed.Name}");
            string fourthFifthMatchResult = random.winProbability(loserOf3rdSeedMatch, fifthSeed);
            Team winnerOf4thFifthSeedMatch = fourthFifthMatchResult.Contains(loserOf3rdSeedMatch.Name) ? loserOf3rdSeedMatch : fifthSeed;
            Team loserOf4thFifthSeedMatch = fourthFifthMatchResult.Contains(loserOf3rdSeedMatch.Name) ? fifthSeed : loserOf3rdSeedMatch; // Get the loser of the match
            Console.WriteLine(fourthFifthMatchResult);
            Console.WriteLine();


            // Upper QF Matches
            Console.WriteLine("Upper QF Matches");
            Console.WriteLine("---------------------");
            Console.WriteLine("Match 1");
            Console.WriteLine($"{firstSeed.Name} vs {winnerOf4thFifthSeedMatch.Name}");
            string upperQFMatch1Result = random.winProbability(firstSeed, winnerOf4thFifthSeedMatch);
            Team winnerOfUpperQF1 = upperQFMatch1Result.Contains(firstSeed.Name) ? firstSeed : winnerOf4thFifthSeedMatch;
            Team loserOfUpperQF1 = upperQFMatch1Result.Contains(firstSeed.Name) ? winnerOf4thFifthSeedMatch : firstSeed;
            Console.WriteLine(upperQFMatch1Result);
            Console.WriteLine();

            Console.WriteLine("Match 2");
            Console.WriteLine($"{secondSeed.Name} vs {winnerOf3rdSeedMatch.Name}");
            string upperQFMatch2Result = random.winProbability(secondSeed, winnerOf3rdSeedMatch);
            Team winnerOfUpperQF2 = upperQFMatch2Result.Contains(secondSeed.Name) ? secondSeed : winnerOf3rdSeedMatch;
            Team loserOfUpperQF2 = upperQFMatch2Result.Contains(secondSeed.Name) ? winnerOf3rdSeedMatch : secondSeed;
            Console.WriteLine(upperQFMatch2Result);
            Console.WriteLine();

            // Lower Bracket Round 1
            Console.WriteLine("Lower Bracket Round 1");
            Console.WriteLine("---------------------");
            Console.WriteLine("Match 1");
            Console.WriteLine($"{eighthSeed.Name} vs {loserOf4thFifthSeedMatch}");
            string lowerBracketMatch1Result = random.winProbability(eighthSeed, loserOf4thFifthSeedMatch);
            Team winnerOfLowerRound1Match1 = lowerBracketMatch1Result.Contains(eighthSeed.Name) ? eighthSeed : loserOf4thFifthSeedMatch;
            Console.WriteLine(lowerBracketMatch1Result);
            Console.WriteLine();

            Console.WriteLine("Match 2");
            Console.WriteLine($"{sixthSeed.Name} vs {seventhSeed.Name}");
            string lowerBracketMatch2Result = random.winProbability(sixthSeed, seventhSeed);
            Team winnerOfLowerRound1Match2 = lowerBracketMatch2Result.Contains(sixthSeed.Name) ? sixthSeed : seventhSeed;
            Console.WriteLine(lowerBracketMatch2Result);
            Console.WriteLine();

            // Lower Bracket Quarterfinals
            Console.WriteLine("Lower Bracket Quarterfinals");
            Console.WriteLine("---------------------");
            Console.WriteLine("Match 1");
            Console.WriteLine($"{loserOfUpperQF1.Name} vs {winnerOfLowerRound1Match1.Name}");
            string lowerQFMatch1Result = random.winProbability(loserOfUpperQF1, winnerOfLowerRound1Match1);
            Team winnerOfLowerQF1 = lowerQFMatch1Result.Contains(loserOfUpperQF1.Name) ? loserOfUpperQF1 : winnerOfLowerRound1Match1;
            Console.WriteLine(lowerQFMatch1Result);
            Console.WriteLine();

            Console.WriteLine("Match 2");
            Console.WriteLine($"{loserOfUpperQF2.Name} vs {winnerOfLowerRound1Match2.Name}");
            string lowerQFMatch2Result = random.winProbability(loserOfUpperQF2, winnerOfLowerRound1Match2);
            Team winnerOfLowerQF2 = lowerQFMatch2Result.Contains(loserOfUpperQF2.Name) ? loserOfUpperQF2 : winnerOfLowerRound1Match2;
            Console.WriteLine(lowerQFMatch2Result);
            Console.WriteLine();

            // Semifinals (Best of 3 Format)
            Console.WriteLine("Semifinals (Best of 3)");
            Console.WriteLine("---------------------");

            // Display who is playing in the semifinals
            Console.WriteLine($"Semifinal 1: {winnerOfUpperQF2.Name} vs {winnerOfLowerQF1.Name}");
            Team semifinalMatch1Winner = BestOf3(random, winnerOfUpperQF2, winnerOfLowerQF1);
            Console.WriteLine($"Semifinal 1 Winner: {semifinalMatch1Winner.Name}");
            Console.WriteLine();

            Console.WriteLine($"Semifinal 2: {winnerOfUpperQF1.Name} vs {winnerOfLowerQF2.Name}");
            Team semifinalMatch2Winner = BestOf3(random, winnerOfUpperQF1, winnerOfLowerQF2);
            Console.WriteLine($"Semifinal 2 Winner: {semifinalMatch2Winner.Name}");
            Console.WriteLine();

            Console.WriteLine($"Worlds Finalists: {semifinalMatch1Winner.Name} vs {semifinalMatch2Winner.Name}");
        }

        // Method to simulate a Best of 3 match
        private Team BestOf3(RandomProbability random, Team team1, Team team2)
        {
            int team1Wins = 0;
            int team2Wins = 0;

            // Best of 3 means first to 2 wins
            while (team1Wins < 2 && team2Wins < 2)
            {
                string matchResult = random.winProbability(team1, team2);
                if (matchResult.Contains(team1.Name))
                {
                    team1Wins++;
                }
                else
                {
                    team2Wins++;
                }
                Console.WriteLine(matchResult);
                Console.WriteLine(); // Adding space between match results
            }

            // Return the winner of the Best of 3 series
            return team1Wins > team2Wins ? team1 : team2;
        }
    }
}
