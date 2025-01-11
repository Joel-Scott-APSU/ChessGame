using System;

namespace SwissBracket
{
    public class Team
    {
        public string Name { get; set; }
        public int EloRating { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int InitialEloRating { get; } // Read-only property for starting Elo

        public Team(string Name, int elo)
        {
            this.Name = Name;
            this.EloRating = elo;
            this.InitialEloRating = elo; // Set starting Elo here
            this.Wins = 0;
            this.Losses = 0;
            this.Points = 0;
        }

        public void AddPoints(int points)
        {
            Points += points;
        }
        public double GetAdjustedWinProbability(Team opponent)
        {
            double ratingDifference = this.EloRating - opponent.EloRating;
            return 1.0 / (1.0 + Math.Pow(10, -ratingDifference / 400));
        }

        public override string ToString()
        {
            return $"{Name} (Elo: {EloRating})";
        }
    }
}


