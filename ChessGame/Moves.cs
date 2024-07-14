using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;

namespace ChessGame
{
    public class Moves
    {
        private Player whitePlayer;
        private Player blackPlayer;
        private Dictionary<Piece, Spot> originalPositions;
        public Moves(Player blackPlayer, Player whitePlayer)
        {
            this.blackPlayer = blackPlayer;
            this.whitePlayer = whitePlayer;
            this.originalPositions = new Dictionary<Piece, Spot>();
        }

        public bool checkForLegalMoves()
        {
            return true;
        }

        public void movePiece(Spot start, Spot end, Piece piece)
        {
            if(!originalPositions.ContainsKey(piece))
            {
                originalPositions[piece] = start;
            }

            
        }
        public bool checkRookMoves(bool isWhite, Piece.Direction direction, Board board, Spot start)
        {

            List<Piece> pieces = isWhite ? whitePlayer.getPieces() : blackPlayer.getPieces();

            foreach (Piece piece in pieces)
            {
                if (Piece.PieceType.Rook == piece.type)
                {
                    int rows = 0;
                    int cols = 0;

                    switch (direction)
                    {
                        case Piece.Direction.North:
                            rows = -1;
                            break;

                        case Piece.Direction.South:
                            rows = 1;
                            break;

                        case Piece.Direction.East:
                            cols = 1;
                            break;

                        case Piece.Direction.West:
                            cols = -1;
                            break;
                    }

                    int pieceRow = start.GetRow();
                    int pieceCol = start.GetColumn();


                }
            }
            return false;
        }
    }
}
