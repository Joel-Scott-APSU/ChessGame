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

        public (bool moveSuccessful, bool enPassantCaptureOccurred) movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;
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
                return (false, false);
            }

            if (!board.isMoveValid(start, end, movingPiece.isWhite()))
            {
                Debug.WriteLine("Move will put king into check");
                return (false, false);
            }

            if (movingPiece.legalMove(board, start, end))
            {
                Debug.WriteLine($"EnPassantPiece: {enPassantPiece} MovingPiece: {movingPiece}");

                (Piece enPassantCapturedPiece, bool enPassantResult) = enPassantCapture(enPassantPiece, movingPiece, toSquare, fromSquare);

                Debug.WriteLine($"EnPassantCapturedPiece: {enPassantCapturedPiece} enPassantResult: {enPassantResult}");

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

                Debug.WriteLine($"\nCurrent Player {currentTurn}\n");

                return (true, enPassantResult);
            }

            return (false, false);
        }


        public (Piece, bool) enPassantCapture(Piece enPassantPiece, Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare)
        {
            // Ensure that the moving piece is a pawn
            if (movingPiece is not Piece.Pawn movingPawn)
            {
                return (null, false);
            }

            Piece capturedPiece = null;
            Spot end = board.getSpot(toSquare.row, toSquare.column);

            Debug.WriteLine($"End Spot on board: {end}");
            // Calculate enPassantRow with boundary check
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;
            Debug.WriteLine($"EnPassantRow: {enPassantRow}");
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

        public bool CastleKingSide(Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare, Board board)
        {
            if(movingPiece is Piece.King king && king.canCastleKingside(king.isWhite(), board))
            {
                Spot kingSpot = board.getSpot(fromSquare.row, fromSquare.column);
                Spot targetSpot = board.getSpot(toSquare.row, 6);

                Spot rookSpot = board.getSpot(fromSquare.row, 7);
                Spot rookTargetSpot = board.getSpot(toSquare.row, 5);


            }
            return false;
        }

        private void swapTurn()
        {
            board.updateThreatMap(currentTurn.getPieces());

            if (board.isKingInCheck(currentTurn.IsWhite))
            {
                Debug.WriteLine("King is in Check, running checkForLegalMoves function");

                (bool canMove, Spot spot) = moves.checkForLegalMoves(currentTurn, board, currentTurn.getPieces());
                if (!canMove)
                {
                    Debug.WriteLine("CHECKMATE");
                }
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
