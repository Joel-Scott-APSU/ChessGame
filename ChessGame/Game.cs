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

            Spot start = board.getSpot(fromRow, fromColumn);
            Spot end = board.getSpot(toRow, toColumn);
            Piece enPassantPiece = board.getSpot(enPassantRow, toColumn).GetPiece();

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

                movingPiece.setCurrentPosition(end);

                currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;

                foreach (Piece enPassant in currentTurn.getPieces())
                {
                    if (enPassant is Piece.Pawn pawn && pawn.isWhite() == currentTurn.IsWhite)
                    {
                        pawn.isEnPassant = false;
                    }
                }

                Debug.WriteLine($"\nCurrent Player {currentTurn}\n");

                return (true, enPassantResult);
            }

            return (false, false);
        }


        public (Piece, bool) enPassantCapture(Piece enPassantPiece, Piece movingPiece, ChessBoardSquare toSquare, ChessBoardSquare fromSquare)
        {
            Piece capturedPiece = null;
            Spot end = board.getSpot(toSquare.row, toSquare.column);
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;

            Debug.WriteLine($"EnPassantPiece: {enPassantPiece} MovingPiece: {movingPiece}");

            if (enPassantPiece != null)
            {
                Piece.Pawn enPassantPawn = enPassantPiece as Piece.Pawn;
                if (enPassantPawn != null && enPassantPawn.isEnPassant)
                {
                    Spot enPassantSpot = board.getSpot(enPassantRow, toSquare.column);
                    enPassantSpot.SetPiece(null);
                    end.SetPiece(movingPiece);
                    capturedPiece = enPassantPawn;
                    return (capturedPiece, true);
                }
            }

            return (null, false);
        }
    }
}
