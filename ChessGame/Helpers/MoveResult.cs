using ChessGame.Models;
using ChessGame.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chess.Helpers
{
    public class MoveResult
    {
        public bool MoveSuccessful { get; set; }
        public bool EnPassantCaptureOccurred { get; set; }
        public bool CastledKingSide { get; set; }
        public bool CastledQueenSide { get; set; }

        public Piece movingPiece { get; set; }
        public Piece capturedPiece { get; set; }

        public ChessBoardSquare fromSquare { get; set; }
        public ChessBoardSquare toSquare { get; set; }
    }
}
