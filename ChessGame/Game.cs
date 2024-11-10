using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Packaging;
using System.Data.Common;

namespace ChessGame
{
    public class Game
    {
        public Board board { get; private set; }
        public Player whitePlayer { get; private set; }
        public Player blackPlayer { get; private set; }
        public Player currentTurn { get; private set; }
        public ChessBoardSquare selectedSquare { get; set; }
        public Moves moves { get; private set; }

        public Game()
        {
            initializeGame();
        }

        private void initializeGame()
        {
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
            board = new Board(whitePlayer, blackPlayer);
            moves = new Moves(whitePlayer, blackPlayer);
            currentTurn = whitePlayer;
        }

        public (bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide) movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;
            bool enPassantResult = false;
            Piece capturedPiece = null;
            Piece enPassantPiece = null;

            Spot start = board.getSpot(fromRow, fromColumn);
            Spot end = board.getSpot(toRow, toColumn);

            if (enPassantRow > 0 && enPassantRow < 7)
            {
                enPassantPiece = board.getSpot(enPassantRow, toColumn).GetPiece();
            }

            Piece movingPiece = start.GetPiece();

            if (movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return (false, false, false, false);
            }

            if (!board.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
                return (false, false, false, false);
            }

            if (movingPiece.legalMove(board, start, end))
            {
                (Piece enPassantCapturedPiece, enPassantResult) = enPassantCapture(enPassantPiece, movingPiece, toSquare, fromSquare);

                if (enPassantResult)
                {
                    capturedPiece = enPassantCapturedPiece;
                }

                if (end.GetPiece() != null)
                {
                    capturedPiece = end.GetPiece();
                }

                if (capturedPiece != null && capturedPiece.isWhite() != movingPiece.isWhite())
                {
                    capturedPiece.setTaken(true);

                    if (currentTurn == whitePlayer)
                    {
                        blackPlayer.removePiece(capturedPiece);
                    }
                    else
                    {
                        whitePlayer.removePiece(capturedPiece);
                    }
                }

                end.SetPiece(movingPiece);
                start.SetPiece(null);

                movingPiece.setCurrentPosition(end);

                swapTurn();

                return (true, enPassantResult, false, false);
            }

            else if (start.GetPiece() != null && start.GetPiece() is Piece.King && end.GetPiece() != null && end.GetPiece() is Piece.Rook)
            {
                if (toSquare.column == 7)
                {
                    if (CastleKingSide(movingPiece, toSquare, fromSquare, board))
                    {
                        start.SetPiece(null);
                        end.SetPiece(null);
                        swapTurn();
                        return (false, false, true, false);
                    }
                }
                else if (toSquare.column == 0)
                {
                    if(CastleQueenSide(movingPiece, toSquare, fromSquare, board))
                    {
                        start.SetPiece(null);
                        end.SetPiece(null);
                        swapTurn();
                        return (false, false, false, true);
                    }
                }
            }


            return (false, false, false, false);
        }


        private (Piece, bool) enPassantCapture(Piece enPassantPiece, Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare)
        {
            // Ensure that the moving piece is a pawn
            if (movingPiece is not Piece.Pawn movingPawn)
            {
                return (null, false);
            }

            Piece capturedPiece = null;
            Spot end = board.getSpot(toSquare.row, toSquare.column);

            // Calculate enPassantRow with boundary check
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;
            if (enPassantRow < 0 || enPassantRow > 7)
            {
                return (null, false);
            }

            // Ensure that the en passant piece is also a pawn
            if (enPassantPiece is Piece.Pawn enPassantPawn && enPassantPawn.isEnPassant)
            {
                Spot enPassantSpot = board.getSpot(enPassantRow, toSquare.column);
                enPassantSpot.SetPiece(null);
                end.SetPiece(movingPiece);
                capturedPiece = enPassantPawn;
                return (capturedPiece, true);
            }

            return (null, false);
        }

        private bool CastleKingSide(Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare, Board board)
        {

            if (movingPiece is Piece.King king && king.canCastleKingside(king.isWhite(), board))
            {
                Piece kingSpot = board.getSpot(fromSquare.row, fromSquare.column).GetPiece();
                Spot targetSpot = board.getSpot(toSquare.row, 6);
                targetSpot.SetPiece(kingSpot);

                Piece rookSpot = board.getSpot(fromSquare.row, 7).GetPiece();

                if (rookSpot is Piece.Rook rook)
                {
                    rook.hasMoved = true;
                }

                Spot rookTargetSpot = board.getSpot(toSquare.row, 5);
                rookTargetSpot.SetPiece(rookSpot);

                king.hasMoved = true;
                return true;
            }

            return false;
        }

        private bool CastleQueenSide(Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare, Board board)
        {
            if (movingPiece is Piece.King king && king.canCastleQueenside(king.isWhite(), board)) { 
                Piece kingSpot = board.getSpot(fromSquare.row, fromSquare.column).GetPiece();
                Spot targetSpot = board.getSpot(toSquare.row, 2);
                targetSpot.SetPiece(kingSpot);

                Piece rookSpot = board.getSpot(fromSquare.row, 0).GetPiece();

                if (rookSpot is Piece.Rook rook)
                {
                    rook.hasMoved = true;
                }

                Spot rookTargetSpot = board.getSpot(toSquare.row, 3);
                rookTargetSpot.SetPiece(rookSpot);

                king.hasMoved = true;
                return true;
            }

            return false;
        }

        private void swapTurn()
        {
            board.updateThreatMap(currentTurn.getPieces());

            if (board.isKingInCheck(currentTurn.IsWhite))
            {
                (bool canMove, Spot spot) = moves.checkForLegalMoves(currentTurn, board, currentTurn.getPieces());
            }

            currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;

            foreach (Piece enPassant in currentTurn.getPieces())
            {
                if (enPassant is Piece.Pawn pawn && pawn.isWhite() == currentTurn.IsWhite)
                {
                    pawn.isEnPassant = false;
                }
            }
        }

    }
}
