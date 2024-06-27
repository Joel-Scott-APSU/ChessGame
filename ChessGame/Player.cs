using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    public class Player
    {
        private bool isWhite;
        private List<Piece> pieces;

        public Player(bool isWhite)
        {
            pieces = new List<Piece>();
            this.isWhite = isWhite;
        }

        public void addPiece(Piece piece)
        {
            pieces.Add(piece);
        }

        public void removePiece(Piece piece)
        {
            pieces.Remove(piece);
        }
        public List<Piece> getPieces()
        {
            return pieces;
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"{(isWhite ? "White Player" : "Black Player")} Pieces:");

            foreach (Piece piece in pieces)
            {
                sb.AppendLine(piece.ToString());
            }

            return sb.ToString();
        }
    }
}
