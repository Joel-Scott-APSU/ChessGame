using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ChessGame.Core;
using static ChessGame.Models.Piece;

namespace ChessGame.Models
{
    public class Moves
    {
        private Game game;
        private ThreatMap threatMap => game.threatMap;

        public Moves(Game game)
        {
            this.game = game;
        }

        public bool checkForLegalMoves(Player player, Board board, IEnumerable<Piece> pieces)
        {
            try
            {
                var bishopDirections = new List<Direction>
        {
            Direction.Northeast,
            Direction.Southwest,
            Direction.Southeast,
            Direction.Northwest
        };

                var rookDirections = new List<Direction>
        {
            Direction.North,
            Direction.South,
            Direction.East,
            Direction.West
        };

                foreach (Piece piece in pieces.ToList())
                {
                    Spot start = piece.getCurrentPosition();
                    if (start == null) continue;

                    switch (piece.type)
                    {
                        case PieceType.Rook:
                            foreach (var direction in rookDirections)
                            {
                                if (checkRookMoves(direction, board, start))
                                {
                                    return true;
                                }
                            }
                            break;

                        case PieceType.Bishop:
                            foreach (var direction in bishopDirections)
                            {
                                if (checkBishopMoves(start, board, direction))
                                {
                                    return true;
                                }
                            }
                            break;

                        case PieceType.Queen:
                            foreach (var direction in rookDirections)
                            {
                                if (checkRookMoves(direction, board, start))
                                {
                                    return true;
                                }
                            }
                            foreach (var direction in bishopDirections)
                            {
                                if (checkBishopMoves(start, board, direction))
                                {
                                    return true;
                                }
                            }
                            break;

                        case PieceType.Knight:
                            if (checkKnightMoves(start, board))
                            {
                                return true;
                            }
                            break;

                        case PieceType.Pawn:
                            if (checkPawnMoves(start, board, piece))
                            {
                                return true;
                            }
                            break;

                        case PieceType.King:
                            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
                            {
                                if (checkKingMoves(start, board, direction))
                                {
                                    Debug.WriteLine("King has legal move in direction " + direction);
                                    return true;
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error checking legal moves: {e}");
            }

            Debug.WriteLine("No legal moves found.");
            return false;
        }


        // === Rook Logic ===
        private bool checkRookMoves(Direction direction, Board board, Spot start)
        {
            int rowDelta = 0, colDelta = 0;
            switch (direction)
            {
                case Direction.North: rowDelta = -1; break;
                case Direction.South: rowDelta = 1; break;
                case Direction.East: colDelta = 1; break;
                case Direction.West: colDelta = -1; break;
                default: return false;
            }

            int r = start.Row + rowDelta;
            int c = start.Column + colDelta;
            while (r >= 0 && r < 8 && c >= 0 && c < 8)
            {
                Spot end = board.GetSpot(r, c);
                if (end.Piece != null && end.Piece.isWhite() == start.Piece.isWhite()) break;

                if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                    return true;

                if (end.Piece != null) break;
                r += rowDelta;
                c += colDelta;
            }
            return false;
        }

        // === Bishop Logic ===
        private bool checkBishopMoves(Spot start, Board board, Direction direction)
        {
            int rowDelta = 0, colDelta = 0;
            switch (direction)
            {
                case Direction.Northeast: rowDelta = -1; colDelta = 1; break;
                case Direction.Northwest: rowDelta = -1; colDelta = -1; break;
                case Direction.Southeast: rowDelta = 1; colDelta = 1; break;
                case Direction.Southwest: rowDelta = 1; colDelta = -1; break;
                default: return false;
            }

            int r = start.Row + rowDelta;
            int c = start.Column + colDelta;
            while (r >= 0 && r < 8 && c >= 0 && c < 8)
            {
                Spot end = board.GetSpot(r, c);
                if (end.Piece != null && end.Piece.isWhite() == start.Piece.isWhite()) break;

                if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                    return true;

                if (end.Piece != null) break;
                r += rowDelta;
                c += colDelta;
            }
            return false;
        }

        // === Knight Logic ===
        private bool checkKnightMoves(Spot start, Board board)
        {
            int[] rowOffsets = { 2, 2, 1, -1, -2, -2, 1, -1 };
            int[] colOffsets = { -1, 1, -2, -2, -1, 1, 2, 2 };

            for (int i = 0; i < 8; i++)
            {
                int row = start.Row + rowOffsets[i];
                int column = start.Column + colOffsets[i];
                if (row < 0 || row > 7 || column < 0 || column > 7) continue;

                Spot end = board.GetSpot(row, column);
                if (end.Piece != null && end.Piece.isWhite() == start.Piece.isWhite()) continue;

                // Check if move puts king in check
                if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                {
                    // Check if the king is still in check after this move
                    bool kingStillInCheck = threatMap.IsKingInCheck(game.currentTurn.IsWhite);
                    if (!kingStillInCheck)
                    {
                        // This move gets the king out of check, so it's legal
                        return true;
                    }
                }
            }
            return false;
        }



        // === King Logic ===
        private bool checkKingMoves(Spot start, Board board, Direction direction)
        {
            int r = start.Row, c = start.Column;
            switch (direction)
            {
                case Direction.North: r -= 1; break;
                case Direction.South: r += 1; break;
                case Direction.East: c += 1; break;
                case Direction.West: c -= 1; break;
                case Direction.Northeast: r -= 1; c += 1; break;
                case Direction.Northwest: r -= 1; c -= 1; break;
                case Direction.Southeast: r += 1; c += 1; break;
                case Direction.Southwest: r += 1; c -= 1; break;
                default: return false;
            }

            if (r < 0 || r > 7 || c < 0 || c > 7) return false;

            Spot end = board.GetSpot(r, c);
            if (end.Piece != null && end.Piece.isWhite() == start.Piece.isWhite()) return false;

            if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                return true;

            return false;
        }

        // === Pawn Logic ===
        private bool checkPawnMoves(Spot start, Board board, Piece pawn)
        {
            int direction = pawn.isWhite() ? -1 : 1;
            int r = start.Row;
            int c = start.Column;

            // Diagonal captures
            foreach (int offset in new[] { -1, 1 })
            {
                int newC = c + offset;
                int newR = r + direction;
                if (newC < 0 || newC > 7 || newR < 0 || newR > 7) continue;

                Spot end = board.GetSpot(newR, newC);
                if (end.Piece != null && end.Piece.isWhite() != pawn.isWhite())
                {
                    if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                        return true;
                }
            }

            // Single forward move
            int oneAhead = r + direction;
            if (oneAhead >= 0 && oneAhead <= 7 && board.GetSpot(oneAhead, c).Piece == null)
            {
                Spot end = board.GetSpot(oneAhead, c);
                if (!threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite))
                    return true;

                // Double forward move
                bool onStartingRow = (pawn.isWhite() && r == 6) || (!pawn.isWhite() && r == 1);
                if (onStartingRow)
                {
                    int twoAhead = r + 2 * direction;
                    if (board.GetSpot(twoAhead, c).Piece == null)
                    {
                        Spot end2 = board.GetSpot(twoAhead, c);
                        if (!threatMap.willMovePutKingInCheck(start, end2, game.currentTurn.IsWhite))
                            return true;
                    }
                }
            }

            // En Passant
            if (PawnEnPassantCheck(start, board, pawn))
                return true;

            return false;
        }

        private bool PawnEnPassantCheck(Spot start, Board board, Piece pawn)
        {
            int row = start.Row;
            int col = start.Column;
            int direction = game.currentTurn.IsWhite ? -1 : 1;

            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int adjCol = col + colOffset;
                if (adjCol < 0 || adjCol > 7) continue;

                Spot adjSpot = board.GetSpot(row, adjCol);
                Piece adjPiece = adjSpot?.Piece;

                if (adjPiece is Pawn enemyPawn && enemyPawn.isWhite() != pawn.isWhite() && enemyPawn.isEnPassant)
                {
                    Spot end = board.GetSpot(row + direction, adjCol);

                    adjSpot.Piece = null;
                    bool legal = !threatMap.willMovePutKingInCheck(start, end, game.currentTurn.IsWhite);
                    adjSpot.Piece = enemyPawn;

                    if (legal) return true;
                }
            }

            return false;
        }
    }
}
