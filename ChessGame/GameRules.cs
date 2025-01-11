using ChessGame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace ChessGame
{
    public class GameRules
    {
        private Game game;

        public GameRules(Game game)
        {
            this.game = game;
        }
        public (bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide) HandleMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Board board = game.board;
            Player currentTurn = game.currentTurn;
            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;
            int enPassantRow = currentTurn.IsWhite ? toSquare.row + 1 : toSquare.row - 1;
            bool enPassantResult;
            Piece? capturedPiece = null;
            Spot? enPassantSpot = null;
            Piece? enPassantPiece = enPassantSpot?.GetPiece();

            Spot start = board.getSpot(fromRow, fromColumn);
            Spot end = board.getSpot(toRow, toColumn);

            if (enPassantRow > 0 && enPassantRow < 7 && board.getSpot(enPassantRow, toColumn).GetPiece() != null)
            {
                enPassantPiece = board.getSpot(enPassantRow, toColumn).GetPiece();
            }

            Piece movingPiece = start?.GetPiece();

            if (movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return (false, false, false, false);
            }

            else if (!board.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
                return (false, false, false, false);
            }

            if (movingPiece.legalMove(board, start, end))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                (Piece enPassantCapturedPiece, enPassantResult) = enPassantCapture(movingPiece, toSquare, board);
#pragma warning restore CS8604 // Possible null reference argument.

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

                    if (currentTurn == game.whitePlayer)
                    {
                        game.blackPlayer.removePiece(capturedPiece);
                    }
                    else
                    {
                        game.whitePlayer.removePiece(capturedPiece);
                    }
                }

                end.SetPiece(movingPiece);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

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
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        end.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        swapTurn();
                        return (false, false, true, false);
                    }
                }
                else if (toSquare.column == 0)
                {
                    if (CastleQueenSide(movingPiece, toSquare, fromSquare, board))
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.SetPiece(null);
                        end.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        swapTurn();
                        return (false, false, false, true);
                    }
                }
            }


            return (false, false, false, false);
        }

        private (Piece, bool) enPassantCapture(Piece movingPiece, ChessBoardSquare toSquare, Board board)
        {
            if(movingPiece is Piece.Pawn pawn)
            {
                Debug.WriteLine(pawn.isEnPassant);
                int direction = movingPiece.isWhite() ? -1 : 1;
                Spot enPassantSpot = board.getSpot(toSquare.row + direction, toSquare.column);
                Piece enPassantPiece = enPassantSpot?.GetPiece();
                if(enPassantPiece is Piece.Pawn enPassantPawn)
                {
                    if(enPassantPawn.isEnPassant && enPassantPawn.isWhite() != movingPiece.isWhite())
                    {
                        return (enPassantPiece, true);
                    }
                }
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
            if (movingPiece is Piece.King king && king.canCastleQueenside(king.isWhite(), board))
            {
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
            game.board.updateThreatMap(game.currentTurn.getPieces());

            if (game.board.isKingInCheck(game.currentTurn.IsWhite))
            {
                (bool canMove, Spot spot) = game.moves.checkForLegalMoves(game.currentTurn, game.board, game.currentTurn.getPieces());
            }

            Player oppositePlayer = game.currentTurn == game.whitePlayer ? game.blackPlayer : game.whitePlayer;
            List<Piece> playerPieces = oppositePlayer.getPieces();
            foreach (Piece piece in playerPieces)
            {
                if(piece is Piece.Pawn pawn)
                {
                    pawn.isEnPassant = false;
                }
            }

            game.SetCurrentTurn(oppositePlayer);
        }

    }
}
