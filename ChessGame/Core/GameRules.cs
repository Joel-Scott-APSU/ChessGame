using ChessGame.Models;
using ChessGame.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using static ChessGame.Models.Piece;

namespace ChessGame.Core
{
    public class GameRules
    {
        private static GameRules? instance;
        private Game game;
        private HashSet<Piece> activePieces;
        private MainWindowViewModel viewModel;
        private ThreatMap threatMap => game.threatMap;
        private Board board => game.board;

        public GameRules(Game game, MainWindowViewModel viewModel)
        {
            this.game = game;
            activePieces = new HashSet<Piece>();
            this.viewModel = viewModel;
        }

        public static GameRules GetInstance(Game game, MainWindowViewModel viewModel)
        {
            if (instance == null)
            {
                instance = new GameRules(game, viewModel);
            }
            return instance;
        }

        public static void ResetInstance()
        {

            instance = null;
        }

        internal static GameRules Create(Game game, MainWindowViewModel viewModel)
        {
            instance = new GameRules(game, viewModel);
            return instance;
        }

        public bool? GetSquareColor(int row, int col)
        {
            return viewModel?.GetSquareColor(row, col);
        }
        public void InitializeActivePieces()
        {
            activePieces = new HashSet<Piece>(game.whitePlayer.GetPieces().Concat(game.blackPlayer.GetPieces()));
        }

        public void InitializeActivePiecesForTest()
        {
            activePieces = new HashSet<Piece>();
        }
        public async Task<(bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide)> HandleMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Board board = game.board;
            Player currentTurn = game.currentTurn;

            Spot start = board.GetSpot(fromSquare.row, fromSquare.column);
            Spot end = board.GetSpot(toSquare.row, toSquare.column);
            Piece? movingPiece = start?.Piece;
            Piece? capturedPiece = null;

            Debug.WriteLine($"Start: {start} End: {end} Moving Piece: {movingPiece} Current Turn: {currentTurn.IsWhite}");

            if (movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return (false, false, false, false);
            }

            else if (threatMap.willMovePutKingInCheck(start, end, movingPiece.isWhite()))
            {
                return (false, false, false, false);
            }


            bool moveSuccessful = false;
            bool enPassantCaptureOccurred = false;
            bool castledKingSide = false;
            bool castledQueenSide = false;

            if (movingPiece.legalMove(threatMap, start, end, board))
            {;
                capturedPiece = end.Piece;


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
                castledKingSide = await PerformCastling(king, toSquare, true, board);
                castledQueenSide = await PerformCastling(king, toSquare, false, board);
                if (toSquare.column == 7 && castledKingSide)
                {
                    Debug.WriteLine("Castled king side");
                    moveSuccessful = true;
                }

                else if (toSquare.column == 0 && castledQueenSide)
                {
                    Debug.WriteLine("Castled queen side");
                    moveSuccessful = true;
                }
            }

            if (moveSuccessful)
            {
                swapTurn();

                if (castledKingSide)
                {
                    string kingsideCastle = "O-O";
                    viewModel.MoveLog.Add(kingsideCastle);
                }
                else if (castledQueenSide)
                {
                    string queensideCastle = "O-O-O";
                    viewModel.MoveLog.Add(queensideCastle);
                }
                else
                {
                    WriteMoveOutput(movingPiece, currentTurn, fromSquare, toSquare, capturedPiece);
                }
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
                    CapturePiece(enPassantPawn);
                    enPassantSpot.Piece = null;
                    return true;
                }
            }

            return false;
        }

        private async Task<bool> PerformCastling(Piece.King king, ChessBoardSquare fromSquare, bool isKingside, Board board)
        {
            // Verify if castling conditions are satisfied
            if ((isKingside && king.canCastleKingside(king.isWhite(), threatMap, board) && king.hasMoved == false) ||
                (!isKingside && king.canCastleQueenside(king.isWhite(), threatMap, board) && king.hasMoved == false))
            {
                try
                {
                    // Define columns for the rook and king's movements
                    int rookColumn = isKingside ? 7 : 0; // Rook's starting column
                    int kingTargetColumn = isKingside ? 6 : 2; // King's target column after castling
                    int rookTargetColumn = isKingside ? 5 : 3; // Rook's target column after castling

                    // Get spots for king, rook, and their target positions
                    Spot kingOriginalSpot = board.GetSpot(fromSquare.row, fromSquare.column);
                    Spot rookSpot = board.GetSpot(fromSquare.row, rookColumn);
                    Spot kingTargetSpot = board.GetSpot(fromSquare.row, kingTargetColumn);
                    Spot rookTargetSpot = board.GetSpot(fromSquare.row, rookTargetColumn);

                    
                    if (rookSpot.Piece is Piece.Rook rook && rook.hasMoved == false)
                    {
                        // Move the king to its target position
                        king.setCurrentPosition(kingTargetSpot);

                        rook.setCurrentPosition(rookTargetSpot);


                        //mark the rook has having moved 
                        rook.hasMoved = true;
                        // Mark the king as having moved
                        king.hasMoved = true;

                        return true; // Castling successful
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error during castling: {ex.Message}");
                }
            }

            return false; // Castling conditions not met
        }


        private void swapTurn()
        {
            // Swap the current turn to the opposite player
            Player currentTurn = game.currentTurn == game.whitePlayer ? game.blackPlayer : game.whitePlayer;

            // Reset the en passant flag for all pawns belonging to the opposite player
            currentTurn.ProcessPawns(pawn => pawn.isEnPassant = false);

            // Set the new current turn
            game.SetCurrentTurn(currentTurn);

            // Update the threat map for the current turn
            game.threatMap.UpdateThreatMap(GetActivePieces(!game.currentTurn.IsWhite));

            viewModel.UpdateTurnDisplay(game.currentTurn);
        }


        public void RemoveActivePiece(Piece piece)
        {
            activePieces.Remove(piece);
        }

        public void AddActivePiece(Piece piece)
        {
            if (piece != null && !activePieces.Contains(piece))
            {
                activePieces.Add(piece);
            }
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
            if (activePieces.Count != 4 || activePieces.Count(p => p.type == Piece.PieceType.Bishop) != 2)
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
            if (activePieces.Count == 4)
            {
                if (DrawKingBishopVKingBishop())
                {
                    return true;
                }
            }

            else if (activePieces.Count == 3)
            {
                if (DrawKingBishopVKing() || DrawKingKnightVKing())
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

        public bool Checkmate(Player player)
        {
            bool canMove = game.moves.checkForLegalMoves(game.currentTurn, game.board, GetActivePieces(!player.IsWhite));

            if (!canMove && threatMap.IsKingInCheck(!player.IsWhite))
            {
                Debug.WriteLine("Checkmate! Game Over.");
                return true;
            }
            else if (!canMove)
            {
                Debug.WriteLine("Stalemate! No legal moves available.");
                return true;
            }

            return false;
        }
        private void PiecePositions(IEnumerable<Piece> activePieces)
        {
            Debug.WriteLine(activePieces.Count());
        }

        public void promotions(Piece piece, Spot promotionSpot, string PromotionType)
        {
            if (piece is Piece.Pawn pawn)
            {
                int row = pawn.getCurrentPosition().Row;
                bool isPromotionRow = (pawn.isWhite() && row == 0) || (!pawn.isWhite() && row == 7);

                if (isPromotionRow)
                {
                    PromotePawn(PromotionType, pawn, promotionSpot);
                }
            }
        }

        public void PromotePawn(string PromotionType, Piece.Pawn pawn, Spot promotionSpot)
        {
            Piece? newPiece = null;

            Player currentPlayer = pawn.isWhite() ? game.whitePlayer : game.blackPlayer;
            Piece promotedPiece = PromotionType switch
            {
                "Queen" => new Piece.Queen(pawn.isWhite()),
                "Rook" => new Piece.Rook(pawn.isWhite()),
                "Bishop" => new Piece.Bishop(pawn.isWhite()),
                "Knight" => new Piece.Knight(pawn.isWhite()),
                _ => throw new ArgumentException("Invalid Promotion Type")
            };

            CapturePiece(pawn); // Remove the pawn from active pieces

            board.CreatePieces(promotedPiece, promotionSpot.Row, promotionSpot.Column, currentPlayer);
        }

        private string ToAlgebraic(int row, int col)
        {
            char file = (char)('a' + col);
            int rank = 8 - row; // Convert to chess notation (1-8)
            return $"{file}{rank}"; // Return in algebraic notation format
        }

        private string GetMoveNotation(Piece movingPiece, ChessBoardSquare fromSquare, ChessBoardSquare toSquare, bool isCapture)
        {
            string toAlgebraic = ToAlgebraic(toSquare.row, toSquare.column);
            string pieceLetter = movingPiece is Piece.Pawn ? "" : GetPieceNotationSymbol(movingPiece);
            string disambiguation = "";

            if (!(movingPiece is Piece.Pawn || movingPiece is Piece.King))
            {
                // Try to find another same-type piece that could have also moved to the square
                string otherFrom = GetDisambiguationSquare(movingPiece, toSquare);
                if (otherFrom != null)
                {
                    // Disambiguation rules: If pieces differ by file, include file; otherwise include rank
                    if (otherFrom[0] != ToAlgebraic(fromSquare.row, fromSquare.column)[0]) // different file
                    {
                        disambiguation = $"{(char)('a' + fromSquare.column)}";
                    }
                    else // same file, so disambiguate by rank
                    {
                        disambiguation = $"{8 - fromSquare.row}";
                    }
                }
            }

            if (movingPiece is Piece.Pawn && isCapture)
            {
                char fromFile = (char)('a' + fromSquare.column);
                return $"{fromFile}x{toAlgebraic}";
            }

            if (isCapture)
                return $"{pieceLetter}{disambiguation}x{toAlgebraic}";

            return $"{pieceLetter}{disambiguation}{toAlgebraic}";
        }


        public string GetPieceNotationSymbol(Piece piece)
        {
            if (piece == null) return "";

            return piece.type switch
            {
                Piece.PieceType.King => "K",
                Piece.PieceType.Queen => "Q",
                Piece.PieceType.Rook => "R",
                Piece.PieceType.Bishop => "B",
                Piece.PieceType.Knight => "N",
                Piece.PieceType.Pawn => "",  // Pawns have no symbol
                _ => ""
            };
        }

        public void WriteMoveOutput(Piece movingPiece, Player currentTurn, ChessBoardSquare fromSquare, ChessBoardSquare toSquare, Piece capturedPiece)
        {
            bool kingInCheck = threatMap.IsKingInCheck(!movingPiece.isWhite());
            bool checkmate = Checkmate(currentTurn);
            string annotation = checkmate ? "#" : (kingInCheck ? "+" : "");
            string moveNotation = $"{GetMoveNotation(movingPiece, fromSquare, toSquare, capturedPiece != null)}{annotation}";
            viewModel.MoveLog.Add(moveNotation);
        }

        private string GetDisambiguationSquare(Piece piece, ChessBoardSquare toSquare)
        {
            foreach (var otherPiece in GetActivePieces(piece.isWhite()))
            {
                // Skip the original moving piece
                if (otherPiece == piece || otherPiece.type != piece.type)
                    continue;

                switch (piece.type)
                {
                    case Piece.PieceType.Knight:
                        if (KnightCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;

                    case Piece.PieceType.Bishop:
                        if (BishopCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;

                    case Piece.PieceType.Rook:
                        if (RookCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;

                    case Piece.PieceType.Queen:
                        if (QueenCanReach(otherPiece, toSquare))
                            return ToAlgebraic(otherPiece.getCurrentPosition().Row, otherPiece.getCurrentPosition().Column);
                        break;
                }
            }

            return null; // No other same-type piece can reach the destination
        }


        private bool KnightCanReach(Piece piece, ChessBoardSquare toSquare)
        {

            Spot spot = piece.getCurrentPosition();
            int startRow = spot.Row;
            int startCol = spot.Column;
            int endRow = toSquare.row;
            int endCol = toSquare.column;

            if(Math.Abs(startRow - endRow) == 2 && Math.Abs(startCol - endCol) == 1 ||
               Math.Abs(startRow - endRow) == 1 && Math.Abs(startCol - endCol) == 2)
            {
                return true; // Knight can reach the square
            }
            return false;
        }

        private bool BishopCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            Spot spot = piece.getCurrentPosition();
            int startRow = spot.Row;
            int startCol = spot.Column;
            int endRow = toSquare.row;
            int endCol = toSquare.column;

            if(Math.Abs(startRow - endRow) != Math.Abs(startCol - endCol))
                return false; // Bishop must move diagonally

            int rowStep = Math.Sign(endRow - startRow);
            int colStep = Math.Sign(endCol - startCol);

            for (int i = 1; i < Math.Abs(startRow - endRow); i++)
            {
                int row = startRow + i * rowStep;
                int col = startCol + i * colStep;
                if (board.GetSpot(row, col).Piece != null)
                    return false; // There is a piece blocking the path
            }

            return true;
        }

        private bool RookCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            Spot spot = piece.getCurrentPosition();
            int startRow = spot.Row;
            int startCol = spot.Column;
            int endRow = toSquare.row;
            int endCol = toSquare.column;

            // Rook must move in a straight line
            if (startRow != endRow && startCol != endCol)
                return false;

            if (startRow == endRow)
            {
                // Horizontal move
                int step = endCol > startCol ? 1 : -1;
                for (int col = startCol + step; col != endCol; col += step)
                {
                    if (board.GetSpot(startRow, col).Piece != null)
                        return false;
                }
                return true;
            }

            if (startCol == endCol)
            {
                // Vertical move
                int step = endRow > startRow ? 1 : -1;
                for (int row = startRow + step; row != endRow; row += step)
                {
                    if (board.GetSpot(row, startCol).Piece != null)
                        return false;
                }
                return true;
            }

            return false;
        }


        private bool QueenCanReach(Piece piece, ChessBoardSquare toSquare)
        {
            return BishopCanReach(piece, toSquare) || RookCanReach(piece, toSquare);
        }
    }
}
