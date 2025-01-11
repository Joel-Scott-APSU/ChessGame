using SwissBracket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SwissBracketSplit2
{
    public class League
    {
        private Team[] teams;
        private List<Team> winningTeams;

        public League(Team[] teams)
        {
            this.teams = teams;
            this.winningTeams = new List<Team>(); // Correctly initialize the list
        }

        public Team[] getTeams()
        {
            return teams;
        }

        public List<Team> getWinningTeam()
        {
            return winningTeams;
        }

        private void simulateLeagueMatch()
        {
            Team[] teams = getTeams();
            RandomProbability random = new();

            for (int i = 0; i < teams.Length - 1; i++)
            {
                for (int j = i + 1; j < teams.Length; j++)
                {
                    String winner = random.winProbability(teams[i], teams[j]);

                    if (winner.Equals(teams[i].Name, StringComparison.OrdinalIgnoreCase))
                    {
                        teams[i].Wins++;
                        teams[j].Losses++;
                    }
                    else
                    {
                        teams[i].Losses++;
                        teams[j].Wins++;
                    }
                }
            }
        }

        public void displayStandings()
        {
            simulateLeagueMatch();

            // Sort teams by Wins in descending order
            Array.Sort(teams, (t1, t2) => t2.Wins.CompareTo(t1.Wins));

            int i = 1;

            Console.WriteLine("League Standings:");
            foreach (Team team in teams)
            {
                Console.WriteLine($"{i}. {team.Name}: Wins = {team.Wins}, Losses = {team.Losses}");
                i++;
            }
            Console.WriteLine("---------------------------------------");

            // Add the top 4 teams to winningTeams
            for (int j = 0; j < Math.Min(4, teams.Length); j++) // Prevent out-of-bounds
            {
                winningTeams.Add(teams[j]);
            }
        }
    }

        public class MultiLeagueSimulator
        {

            public  List<Team> teams1;
            public  List<Team> teams2;
            public  List<Team> teams3;
            public  List<Team> teams4;

            public MultiLeagueSimulator()
            {
                teams1 = new List<Team>
{
    new Team("RB Leipzig", 1880),
    new Team("Fenerbahçe", 1860),
    new Team("Emelec", 1650),
    new Team("Bayern Munich", 1950),
    new Team("Rangers", 1790),
    new Team("Atlético Mineiro", 1740),
    new Team("Dortmund", 1935),
    new Team("Panathinaikos", 1705),
    new Team("Zenit", 1810),
    new Team("Corinthians", 1750),
    new Team("Brazil", 1975),
    new Team("Soccer Aid", 2280),
    new Team("Racing Club", 1660),
    new Team("Atletico", 1925),
    new Team("Germany", 1960),
    new Team("Sporting CP", 1870),
    new Team("France", 1965),
    new Team("Anderlecht", 1720),
    new Team("Benfica", 1880),
    new Team("Ajax", 1895),
};


                teams2 = new List<Team>
{
    new Team("Beşiktaş", 1695),
    new Team("Inter", 1900),
    new Team("Adidas All Stars", 2275),
    new Team("Argentina", 1960),
    new Team("AC Milan", 1920),
    new Team("Club Brugge", 1725),
    new Team("Villarreal CF", 1890),
    new Team("Vasco da Gama", 1685),
    new Team("Manchester City", 1945),
    new Team("Independiente", 1655),
    new Team("Internacional", 1675),
    new Team("Portugal", 1975),
    new Team("Dynamo Kyiv", 1815),
    new Team("Roma", 1910),
    new Team("Manchester United", 1940),
    new Team("Celtic", 1795),
    new Team("Santos", 1760),
    new Team("Napoli", 1925),
    new Team("CSKA Moscow", 1800),
    new Team("Marseille", 1840),
};


                teams3 = new List<Team>
{
    new Team("Monaco", 1825),
    new Team("RB Bragantino", 1665),
    new Team("Lazio", 1920),
    new Team("PSG", 1940),
    new Team("PSV", 1735),
    new Team("Barcelona", 1950),
    new Team("AEK Athens", 1700),
    new Team("Spain", 1950),
    new Team("Porto", 1785),
    new Team("Inter Miami", 2300),
    new Team("River Plate", 1765),
    new Team("Galatasaray", 1850),
    new Team("Genk", 1715),
    new Team("Spartak Moscow", 1805),
    new Team("Red Bull Salzburg", 1670),
    new Team("Universidad Católica", 1640),
    new Team("São Paulo", 1680),
    new Team("Arsenal", 1915),
    new Team("England", 1960),
    new Team("Real Madrid", 1985),
};


                teams4 = new List<Team>
{
    new Team("Feyenoord", 1730),
    new Team("Belgium", 1945),
    new Team("Italy", 1950),
    new Team("Boca Juniors", 1770),
    new Team("Liverpool", 1930),
    new Team("Bergamo Calcio", 1935),
    new Team("Grêmio", 1745),
    new Team("Leicester City", 1875),
    new Team("Real Sociedad", 1910),
    new Team("Palmeiras", 1775),
        new Team("New England", 2325),
    new Team("Olympiacos", 1710),
    new Team("Cruzeiro", 1755),
    new Team("Flamengo", 1780),
    new Team("Shakhtar Donetsk", 1820),
    new Team("Sevilla", 1835),
    new Team("Chelsea", 1930),
    new Team("Trabzonspor", 1690),
    new Team("Liga de Quito", 1645),
    new Team("Lyon", 1830),
};
            }
        // Method to retrieve a team by name
        public Team GetTeamByName(string teamName)
        {
            // Search all leagues for the team
            foreach (var league in new List<List<Team>> { teams1, teams2, teams3, teams4 })
            {
                Team team = league.FirstOrDefault(t => t.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase));
                if (team != null)
                {
                    return team;
                }
            }

            return null; // Return null if team not found
        }
        public List<Team> SimulateAllLeagues()
            {
                
                List<Team>[] allLeagues = { teams1, teams2, teams3, teams4 };
            foreach (var league in allLeagues)
            {
                foreach (Team team in league)
                {
                    team.Wins = 0;
                    team.Losses = 0;
                }
            }
                List<Team> top16Teams = new(); // List to store the top 4 teams from each league

                for (int leagueIndex = 0; leagueIndex < allLeagues.Length; leagueIndex++)
                {
                    Console.WriteLine($"\nLeague {leagueIndex + 1} Results:");
                    Team[] leagueTeams = allLeagues[leagueIndex].ToArray();
                    League simulator = new League(leagueTeams);
                    simulator.displayStandings();

                    // Add the top 4 teams to the consolidated list
                    top16Teams.AddRange(simulator.getWinningTeam().Take(4)); // Take only top 4 teams
                }

                // Sort by wins (descending), then by Elo (descending)
                var sortedTop16Teams = top16Teams
                    .OrderByDescending(team => team.Wins)
                    .ThenByDescending(team => team.InitialEloRating)
                    .ToList();

                // Display all top 16 teams at the end
                Console.WriteLine("\nTop 16 Teams Across All Leagues (Sorted):");
                int rank = 1;
                foreach (var team in sortedTop16Teams)
                {
                    Console.WriteLine($"{rank}. {team.Name}: Wins = {team.Wins}, Losses = {team.Losses}");
                    rank++;
                }

                return sortedTop16Teams;
            }


        }
    }

