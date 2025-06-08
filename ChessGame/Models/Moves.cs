using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Windows.Input;
using static ChessGame.Piece;

namespace ChessGame
{
    public class Moves
    {
        private Player whitePlayer;
        private Player blackPlayer;
        private GameRules gameRules;

        public Moves(Player whitePlayer, Player blackPlayer, GameRules gameRules)
        {
            this.blackPlayer = blackPlayer;
            this.whitePlayer = whitePlayer;
            this.gameRules = gameRules;
        }

        public bool checkForLegalMoves(Player player, Board board, IEnumerable<Piece> pieces)
        { 
            try
            {
                var bishopDirections = new List<Piece.Direction>
                {
                    Piece.Direction.Northeast,
                    Piece.Direction.Southwest,
                    Piece.Direction.Southeast,
                    Piece.Direction.Northwest
                };

                var rookDirections = new List<Piece.Direction>
                {
                    Piece.Direction.North,
                    Piece.Direction.South,
                    Piece.Direction.East,
                    Piece.Direction.West
                };

                foreach (Piece piece in pieces.ToList())
                {
                    Debug.WriteLine($"Piece in checkForLegalMoves: {piece}");
                    Spot start = piece.getCurrentPosition();

                    switch (piece.type)
                    {
                        case Piece.PieceType.Rook:
                            foreach (var direction in rookDirections)
                            {
                                if (checkRookMoves(player, direction, board, start))
                                {
                                    Debug.WriteLine($"Rook can move in Direction: {direction}");
                                    return true;
                                }
                            }
                            break;
                            
                        case Piece.PieceType.Pawn:
                            if (checkPawnMoves(player, start, board, piece))
                            {
                                Debug.WriteLine($"Pawn can move");
                                return true;
                            }
                            break;
                            
                        case Piece.PieceType.Knight:
                            if (checkKnightMoves(player, start, board))
                            {
                                Debug.WriteLine($"Knight can move");
                                return true;
                            }
                            break;
                            
                        case Piece.PieceType.Bishop:
                            foreach (var direction in bishopDirections)
                            {
                                if (checkBishopMoves(player, start, board, direction))
                                {
                                    Debug.WriteLine($"Bishop can move in Direction: {direction}");
                                    return true;
                                }
                            }
                            break;

                        case Piece.PieceType.Queen:
                            foreach (var direction in rookDirections)
                            {
                                if (checkRookMoves(player, direction, board, start))
                                {
                                    Debug.WriteLine($"Queen can move in Direction: {direction}");
                                    return true;
                                }
                            }
                            foreach (var direction in bishopDirections)
                            {
                                if (checkBishopMoves(player, start, board, direction))
                                {
                                    Debug.WriteLine($"Queen can move in Direction: {direction}");
                                    return true;
                                }
                            }
                            break;

                        case Piece.PieceType.King:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                if (checkKingMoves(player, start, board, direction))
                                {
                                    Debug.WriteLine($"King can move in Direction: {direction}");
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
            return false;
        }

        private bool checkRookMoves(Player player, Piece.Direction direction, Board board, Spot start)
        {
            int rowDelta = 0, colDelta = 0;

            switch (direction)
            {
                case Piece.Direction.North: rowDelta = -1; break;
                case Piece.Direction.South: rowDelta = 1; break;
                case Piece.Direction.East: colDelta = 1; break;
                case Piece.Direction.West: colDelta = -1; break;
                default: return false;
            }

            int pieceRow = start.Row + rowDelta;
            int pieceCol = start.Column + colDelta;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.GetSpot(pieceRow, pieceCol);
                Piece currentPiece = currentSpot.Piece;

                if (currentPiece != null && currentPiece.isWhite() != start.Piece.isWhite())
                {
                    if (!board.willMovePutKingInCheck(start, currentSpot, player.IsWhite))
                        return true;
                    break;
                }
                else if (currentPiece == null)
                {
                    if (!board.willMovePutKingInCheck(start, currentSpot, player.IsWhite))
                        return true;
                }
                else
                {
                    break;
                }

                pieceRow += rowDelta;
                pieceCol += colDelta;
            }

            return false;
        }
        
        private bool checkPawnMoves(Player player, Spot start, Board board, Piece pawn)
        {
            int direction = player.IsWhite ? -1 : 1;
            int row = start.Row;
            int col = start.Column;

            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int newCol = col + colOffset;
                int newRow = row + direction;

                if (newCol >= 0 && newCol < 8 && newRow >= 0 && newRow < 8)
                {
                    Spot attackSpot = board.GetSpot(newRow, newCol);
                    Piece targetPiece = attackSpot.Piece;

                    if (targetPiece != null && targetPiece.isWhite() != pawn.isWhite())
                    {
                        if (!board.willMovePutKingInCheck(start, attackSpot, player.IsWhite))
                            return true;
                    }
                }
            }

            int forwardRow = row + direction;
            if (forwardRow >= 0 && forwardRow < 8 && board.GetSpot(forwardRow, col).Piece == null)
            {
                if (!board.willMovePutKingInCheck(start, board.GetSpot(forwardRow, col), player.IsWhite))
                    return true;
            }

            if ((pawn.isWhite() && row == 6) || (!pawn.isWhite() && row == 1))
            {
                int middleRow = row + direction;
                int doubleMoveRow = row + 2 * direction;

                if (board.GetSpot(middleRow, col).Piece == null && board.GetSpot(doubleMoveRow, col).Piece == null)
                {
                    if (!board.willMovePutKingInCheck(start, board.GetSpot(doubleMoveRow, col), player.IsWhite))
                        return true;
                }
            }

            return false;
        }
        

        private bool checkKnightMoves(Player player, Spot start, Board board)
        {
            int[] rowOffsets = { 2, 2, 1, -1, -2, -2, 1, -1 };
            int[] colOffsets = { -1, 1, -2, -2, -1, 1, 2, 2 };

            for (int i = 0; i < rowOffsets.Length; i++)
            {
                int newRow = start.Row + rowOffsets[i];
                int newCol = start.Column + colOffsets[i];

                // Ensure the move is within bounds of the board
                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    Spot moveSpot = board.GetSpot(newRow, newCol);
                    Piece targetPiece = moveSpot.Piece;
                    Piece startPiece = start.Piece;

                    // Check if the target spot is either empty or contains an opponent's piece
                    if (targetPiece == null || targetPiece.isWhite() != startPiece.isWhite())
                    {
                        // Simulate the move and check if it puts the king in check
                        if (!board.willMovePutKingInCheck(start, moveSpot, player.IsWhite))
                        {
                            // Check if the king is in check after the move (important!)
                            bool kingInCheck = board.IsKingInCheck(player.IsWhite);

                            // If the king is not in check, it's a valid move
                            if (!kingInCheck)
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }


        private bool checkBishopMoves(Player player, Spot start, Board board, Piece.Direction direction)
        {
            int colDelta;
            int rowDelta;
            switch (direction)
            {
                case Piece.Direction.Northeast: rowDelta = -1; colDelta = 1; break;
                case Piece.Direction.Northwest: rowDelta = -1; colDelta = -1; break;
                case Piece.Direction.Southeast: rowDelta = 1; colDelta = 1; break;
                case Piece.Direction.Southwest: rowDelta = 1; colDelta = -1; break;
                default: return false;
            }

            int pieceRow = start.Row + rowDelta;
            int pieceCol = start.Column + colDelta;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.GetSpot(pieceRow, pieceCol);
                Piece pieceAtSpot = currentSpot.Piece;

                Debug.WriteLine("Checking Location: " + currentSpot);
                // If there is a piece of the same color, stop the movement
                if (pieceAtSpot != null && pieceAtSpot.isWhite() == start.Piece.isWhite())
                    break;

                // Simulate the move and check if it would put the king in check
                if (!board.willMovePutKingInCheck(start, currentSpot, player.IsWhite))
                {
                    // Check if the king is in check after the simulated move (important!)
                    bool kingInCheck = board.IsKingInCheck(player.IsWhite);
                    if (!kingInCheck)
                    {
                        return true;  // The move is legal, as it doesn't put the king in check
                    }
                }

                // If the piece at the spot is an opponent's piece, stop the movement
                if (pieceAtSpot != null && pieceAtSpot.isWhite() != start.Piece.isWhite())
                    break;

                // Continue moving in the current direction
                pieceRow += rowDelta;
                pieceCol += colDelta;
            }
            return false;
        }


        private bool checkKingMoves(Player player, Spot start, Board board, Piece.Direction direction)
        {
            int row = start.Row, col = start.Column;
            int newRow = row, newCol = col;

            // Calculate the target position based on the direction
            switch (direction)
            {
                case Piece.Direction.North: newRow -= 1; break;
                case Piece.Direction.South: newRow += 1; break;
                case Piece.Direction.East: newCol += 1; break;
                case Piece.Direction.West: newCol -= 1; break;
                case Piece.Direction.Northeast: newRow -= 1; newCol += 1; break;
                case Piece.Direction.Northwest: newRow -= 1; newCol -= 1; break;
                case Piece.Direction.Southeast: newRow += 1; newCol += 1; break;
                case Piece.Direction.Southwest: newRow += 1; newCol -= 1; break;
                default: return false;
            }

            // Check bounds
            if (newRow < 0 || newRow >= 8 || newCol < 0 || newCol >= 8)
                return false;

            Spot moveSpot = board.GetSpot(newRow, newCol);
            Piece targetPiece = moveSpot.Piece;
            
            
            // Don't move into a square occupied by a friendly piece
            if (targetPiece != null && targetPiece.isWhite() == start.Piece.isWhite())
                return false;

            // Simulate the move and check if the king would still be in check
            if (!board.willMovePutKingInCheck(start, moveSpot, player.IsWhite))
            {
                Debug.WriteLine("King can legally move to: " + moveSpot);
                return true;
            }

            return false;
        }



    }
}