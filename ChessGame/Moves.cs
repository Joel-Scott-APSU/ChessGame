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
using static ChessGame.Piece;

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

        public bool checkForLegalMoves(bool isWhite, Board board, Spot start)
        {
            List<Piece> pieces = isWhite ? whitePlayer.getPieces() : blackPlayer.getPieces();

            foreach(Piece piece in pieces)
            {
                switch (piece.type)
                {
                    case Piece.PieceType.Rook:
                        foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                        {
                            checkRookMoves(isWhite, direction, board, start);
                        }
                        break;

                    case Piece.PieceType.Pawn:
                        checkPawnMoves(isWhite, start, board, piece);
                        break;


                }
            }

            return true;
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

                    int pieceRow = start.GetRow() + rows;
                    int pieceCol = start.GetColumn() + cols;

                    while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
                    {
                        Spot currentSpot = board.getSpot(pieceRow, pieceCol);
                        Piece currentPiece = currentSpot.GetPiece();

                        // Check if there's a piece obstructing the rook's path
                        if (currentPiece != null)
                        {
                            if (currentPiece.isWhite() != isWhite)
                            {
                                // Opponent's piece found, simulate capture and move
                                Piece capturedPiece = currentPiece;
                                currentSpot.SetPiece(piece); // Simulate capturing the piece
                                start.SetPiece(null); // Simulate moving the rook from start

                                // Check if this move gets the king out of check
                                if (!board.isKingInCheck(isWhite))
                                {
                                    // Restore board state
                                    currentSpot.SetPiece(capturedPiece); // Restore the captured piece
                                    start.SetPiece(piece); // Restore the rook to start
                                    return true; // Rook can capture this piece and get the king out of check
                                }

                                // Restore board state after simulation
                                currentSpot.SetPiece(capturedPiece);
                                start.SetPiece(piece);
                            }
                            break; // There's an obstruction, stop checking in this direction
                        }
                        else // Empty spot, simulate moving the rook there
                        {
                            currentSpot.SetPiece(piece); // Simulate moving the rook to the empty spot
                            start.SetPiece(null); // Simulate moving the rook from start

                            // Check if this move gets the king out of check
                            if (!board.isKingInCheck(isWhite))
                            {
                                // Restore board state after simulation
                                currentSpot.SetPiece(null);
                                start.SetPiece(piece);
                                return true; // Rook can move to this spot and get the king out of check
                            }

                            // Restore board state after simulation
                            currentSpot.SetPiece(null);
                            start.SetPiece(piece);
                        }

                        pieceRow += rows;
                        pieceCol += cols;
                    }
                }
            }
            return false;
        }

        public bool checkPawnMoves(bool isWhite, Spot start, Board board, Piece pawn)
        {
            int row = start.GetRow();
            int col = start.GetColumn();
            int startPoint = isWhite ? 6 : 1;
            int direction = pawn.isWhite() ? -1 : 1; 
            Piece piece = start.GetPiece();

            if (row + direction >= 0 && row + direction < 8)
            {
                if (col - 1 >= 0)
                {
                    Spot attackingSpot = board.getSpot(row + direction, col - 1);
                    if (attackingSpot.GetPiece() != null && attackingSpot.GetPiece().isWhite() != isWhite)
                    {
                        Piece capturedPiece = attackingSpot.GetPiece();
                        attackingSpot.SetPiece(piece);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(piece);
                            attackingSpot.SetPiece(capturedPiece);
                            return true;
                        }

                        start.SetPiece(piece);
                        attackingSpot.SetPiece(capturedPiece);
                    }
                }

                if (col + 1 < 8)
                {
                    Spot attackingSpot = board.getSpot(row + direction, col + 1);
                    if(attackingSpot.GetPiece() != null && attackingSpot.GetPiece().isWhite() != isWhite)
                    {
                        Piece capturedPiece = attackingSpot.GetPiece();
                        attackingSpot.SetPiece(piece);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(piece);
                            attackingSpot.SetPiece(capturedPiece);
                            return true;
                        }

                        start.SetPiece(piece);
                        attackingSpot.SetPiece(capturedPiece);
                    }
                }
            }

            if (start.GetPiece().isWhite())
            {
                for(int i = -1; i > -3; i--)
                {
                    Spot pawnSpot = board.getSpot(row + i, col);

                    if(pawnSpot.GetPiece() != null)
                    {
                        return false;
                    }

                    pawnSpot.SetPiece(piece);
                    start.SetPiece(null);

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(piece);
                        pawnSpot.SetPiece(null);
                        return true;
                    }

                    start.SetPiece(piece);
                    pawnSpot.SetPiece(null);

                    if (startPoint != start.GetRow())
                    {
                        break;
                    }
                }
            }

            else
            {
                for(int i = 1; i < 3; i++)
                {
                    Spot pawnSpot = board.getSpot(row + i, col);

                    if(pawnSpot.GetPiece() != null)
                    {
                        return false;
                    }

                    pawnSpot.SetPiece(piece);
                    start.SetPiece(null);

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(piece);
                        pawnSpot.SetPiece(null);
                        return true;
                    }

                    start.SetPiece(piece);
                    pawnSpot.SetPiece(null);

                    if (startPoint != start.GetRow())
                    {
                        break;
                    }
                }
            }
            return false;
        }
    }
}
