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
        MainWindowViewModel viewModel;

        public GameRules(Game game, MainWindowViewModel viewModel)
        {
            this.game = game;
            this.activePieces = new HashSet<Piece>();
            this.viewModel = viewModel;
        }

        public bool? GetSquareColor(int row, int col)
        {
            return viewModel?.GetSquareColor(row, col);
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
                return (false, false, false, false);
            }

            else if (!board.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
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
                PiecePositions(GetActivePieces(currentTurn.IsWhite));
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
            bool canMove = true;
            // Update the threat map for the current turn
            game.board.UpdateThreatMap(GetActivePieces(game.currentTurn.IsWhite));
            Debug.WriteLine($"is white player: {game.currentTurn.IsWhite}");

            Player oppositeTurn = game.currentTurn == game.whitePlayer ? game.blackPlayer : game.whitePlayer;

            // Check if the king is in check
            if (game.board.IsKingInCheck(oppositeTurn.IsWhite))
            {
                Debug.WriteLine("Checking if the king is in checkmate");
                (canMove, Spot spot) = game.moves.checkForLegalMoves(game.currentTurn, game.board, GetActivePieces(!game.currentTurn.IsWhite));
                foreach (Piece piece in GetActivePieces(!game.currentTurn.IsWhite))
                {
                    Debug.WriteLine($"Pieces used in check for legal moves: {piece}");
                }
            }

            Debug.WriteLine($"Can Move: {canMove}");
            if(!canMove)
            {
                Debug.WriteLine("CHECKMATE");
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

        private bool DrawKingBishopVKing()
        {
            return activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Bishop);
        }

        private bool DrawKingBishopVKingBishop()
        {
            if(activePieces.Count == 4 || activePieces.Count(p => p.type == Piece.PieceType.Bishop) != 2)
            {
                return false;
            }

            var bishops = activePieces.Where(p => p.type == Piece.PieceType.Bishop).ToList();

            Spot position1 = bishops[0].getCurrentPosition();
            Spot position2 = bishops[1].getCurrentPosition();

            int row1 = position1.Row;
            int col1 = position1.Column;
            int row2 = position2.Row;
            int col2 = position2.Column;

            bool? color1 = GetSquareColor(row1, col1);
            bool? color2 = GetSquareColor(row2, col2);

            return color1.HasValue && color2.HasValue && color1.Value == color2.Value;
        }

        private bool DrawKingKnightVKing()
        {
            return activePieces.Count == 3 && activePieces.Any(p => p.type == Piece.PieceType.Knight);
        }

        public bool Draw()
        {
            if(activePieces.Count == 4)
            {
                return DrawKingBishopVKingBishop();
            }

            else if(activePieces.Count == 3)
            {
                if(DrawKingBishopVKing() || DrawKingKnightVKing())
                {
                    return true;
                }
            }

            else if (activePieces.Count == 2)
            {
                return true;
            }


            return false;
        }

        private void PiecePositions(IEnumerable<Piece> activePieces)
        {
            foreach(Piece piece in activePieces)
            {
                Debug.WriteLine($"Piece: {piece} Piece Position: {piece.getCurrentPosition()}");
            }
            Debug.WriteLine(activePieces.Count());
        }
    }
}
