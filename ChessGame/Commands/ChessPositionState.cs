using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Commands
{
    public class ChessPositionState : IEquatable<ChessPositionState>
    {
        public string BoardFEN { get; }
        public string ActiveColor { get; }
        public string CastlingAvailability { get; }
        public string EnPassantTargetSquare { get; }

        public ChessPositionState(string boardFEN, string activeColor, string castlingAvailability,
                                  string enPassantTargetSquare)
        {
            BoardFEN = boardFEN;
            ActiveColor = activeColor;
            CastlingAvailability = castlingAvailability;
            EnPassantTargetSquare = enPassantTargetSquare;
        }

        public override bool Equals(object obj) => Equals(obj as ChessPositionState);

        public bool Equals(ChessPositionState? other)
        {
            if (other is null) return false;
            return BoardFEN == other.BoardFEN &&
                   ActiveColor == other.ActiveColor &&
                   CastlingAvailability == other.CastlingAvailability &&
                   EnPassantTargetSquare == other.EnPassantTargetSquare;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(BoardFEN, ActiveColor, CastlingAvailability,
                                    EnPassantTargetSquare);
        }
    }
}
