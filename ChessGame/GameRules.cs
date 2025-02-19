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

        private HashSet<Piece> activePieces;
        public GameRules(Game game)
        {
            this.game = game;
            this.activePieces = new HashSet<Piece>();
        }

        public void InitializeActivePieces()
        {
            activePieces = new HashSet<Piece>(game.whitePlayer.GetPieces().Concat(game.blackPlayer.GetPieces()));
        }
        public (bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide) HandleMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Board board = game.board;
            Player currentTurn = game.currentTurn;

            Debug.WriteLine($"Current Turn {currentTurn}");
            Spot start = board.GetSpot(fromSquare.row, fromSquare.column);
            Spot end = board.GetSpot(toSquare.row, toSquare.column);
            Piece? movingPiece = start?.Piece;

            if (movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                Debug.WriteLine("test 1");
                return (false, false, false, false);
            }

            else if (!board.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
                Debug.WriteLine("Test 2");
                return (false, false, false, false);
            }


            bool moveSuccessful = false;
            bool enPassantCaptureOccurred = false;
            bool castledKingSide = false;
            bool castledQueenSide = false;

            if (movingPiece.legalMove(board, start, end))
            {
                Piece? capturedPiece = end.Piece;

                if (movingPiece is Piece.Pawn && enPassantCapture(movingPiece, toSquare, board, out Piece? enPassantCapturedPiece))
                {
                    capturedPiece = enPassantCapturedPiece;
                    enPassantCaptureOccurred = true;
                }

                if (capturedPiece != null && capturedPiece.isWhite() != movingPiece.isWhite())
                {
                    CapturePiece(capturedPiece);
                }

                end.Piece = movingPiece;
                start.Piece = null;
                movingPiece.setCurrentPosition(end);

                moveSuccessful = true;
            }
            else if (movingPiece is Piece.King king && end.Piece is Piece.Rook)
            {
                if (toSquare.column == 7 && PerformCastling(king, toSquare, true, board))
                {
                    moveSuccessful = true;
                    castledKingSide = true;
                }
                else if (toSquare.column == 0 && PerformCastling(king, toSquare, false, board))
                {
                    moveSuccessful = true;
                    castledQueenSide = true;
                }
            }

            Debug.WriteLine($"Move Successful: {moveSuccessful}");
            if (moveSuccessful)
            {
                swapTurn(); 
            }

            return (moveSuccessful, enPassantCaptureOccurred, castledKingSide, castledQueenSide);
        }


        private bool enPassantCapture(Piece movingPiece, ChessBoardSquare toSquare, Board board, out Piece enPassantCapturedPiece)
        {
            enPassantCapturedPiece = null;

            if (movingPiece is Piece.Pawn pawn)
            {
                int direction = movingPiece.isWhite() ? 1 : -1;
                Spot enPassantSpot = board.GetSpot(toSquare.row + direction, toSquare.column);
                Piece? enPassantPiece = enPassantSpot?.Piece;

                if (enPassantPiece is Piece.Pawn enPassantPawn && enPassantPawn.isEnPassant && enPassantPawn.isWhite() != pawn.isWhite())
                {
                    enPassantCapturedPiece = enPassantPiece;
                    return true;
                }
            }

            return false;
        }

        private bool PerformCastling(Piece.King king, ChessBoardSquare fromSquare, bool isKingside, Board board)
        {
            // Verify if castling conditions are satisfied
            if ((isKingside && king.canCastleKingside(king.isWhite(), board)) ||
                (!isKingside && king.canCastleQueenside(king.isWhite(), board)))
            {
                // Define columns for the rook and king's movements
                int rookColumn = isKingside ? 7 : 0; // Rook's starting column
                int kingTargetColumn = isKingside ? 6 : 2; // King's target column after castling
                int rookTargetColumn = isKingside ? 5 : 3; // Rook's target column after castling

                // Get spots for king, rook, and their target positions
                Spot kingOriginalSpot = board.GetSpot(fromSquare.row, fromSquare.column);
                Spot rookSpot = board.GetSpot(fromSquare.row, rookColumn);
                Spot kingTarGetSpot = board.GetSpot(fromSquare.row, kingTargetColumn);
                Spot rookTarGetSpot = board.GetSpot(fromSquare.row, rookTargetColumn);

                Debug.WriteLine($"king Target Spot {kingTarGetSpot} Rook Target Spot {rookTarGetSpot}");
                // Move the king to its target position
                kingTarGetSpot.Piece = king;
                kingOriginalSpot.Piece = null; // Clear king's original spot

                // Move the rook to its target position
                if (rookSpot.Piece is Piece.Rook castlingRook)
                {
                    Debug.WriteLine("Piece is Rook");
                    rookTarGetSpot.Piece = castlingRook; // Assign rook to target spot
                    rookSpot.Piece = null;              // Clear rook's original spot
                    castlingRook.hasMoved = true;       // Mark rook as having moved
                }

                // Mark the king as having moved
                king.hasMoved = true;

                return true; // Castling successful
            }

            return false; // Castling conditions not met
        }


        private void swapTurn()
        {
            // Update the threat map for the current turn
            game.board.UpdateThreatMap(GetActivePieces(game.currentTurn.IsWhite));

            // Check if the king is in check
            if (game.board.IsKingInCheck(game.currentTurn.IsWhite))
            {
                (bool canMove, Spot spot) = game.moves.checkForLegalMoves(game.currentTurn, game.board, game.currentTurn.GetPieces());
            }

            // Swap the current turn to the opposite player
            Player currentTurn = game.currentTurn == game.whitePlayer ? game.blackPlayer : game.whitePlayer;

            // Reset the en passant flag for all pawns belonging to the opposite player
            currentTurn.ProcessPawns(pawn => pawn.isEnPassant = false);

            // Set the new current turn
            Debug.WriteLine($"Swapping turn. Current turn before swap: {(game.currentTurn.IsWhite ? "White" : "Black")}");
            game.SetCurrentTurn(currentTurn);
            Debug.WriteLine(currentTurn);
        }

        public void RemoveActivePiece(Piece piece)
        {
            activePieces.Remove(piece);
        }

        public void AddActivePiece(Piece piece)
        {
            activePieces.Add(piece);
        }

        public IEnumerable<Piece> GetActivePieces(bool isWhite)
        {
            return activePieces?.Where(piece => piece.isWhite() == isWhite) ?? [];
        }


        public void CapturePiece(Piece piece)
        {
            RemoveActivePiece(piece);
        }

        public bool DrawKingVKing()
        {
            IEnumerable<Piece> pieces = GetActivePieces(game.currentTurn.IsWhite);
            IEnumerable<Piece> opponentPieces = GetActivePieces(!game.currentTurn.IsWhite);

                if(pieces.Count() == 1 && pieces.First().type is Piece.PieceType.King && 
                opponentPieces.Count() == 1 && opponentPieces.First().type == Piece.PieceType.King)
                {
                    return true;
                }

            return false;
        }

        public bool DrawKingBishopVKing()
        {
            return activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Bishop);
        }

        public bool DrawKingBishopVKingBishop()
        {
            return activePieces.Count == 4 && activePieces.Count(p => p.type == Piece.PieceType.Bishop) == 2;
        }

        public bool DrawKingKnightVKing()
        {
            return activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Knight);
        }
    }
}
