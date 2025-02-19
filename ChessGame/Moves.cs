using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

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

        public (bool, Spot) checkForLegalMoves(Player player, Board board, IReadOnlyList<Piece> pieces)
        {
            try
            {
                foreach (Piece piece in pieces)
                {

                    Spot start = piece.getCurrentPosition();

                    switch (piece.type)
                    {
                        case Piece.PieceType.Rook:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                var (canMove, spot) = checkRookMoves(player, direction, board, start);

                                    return (true, start);
                                
                            }
                            break;

                        case Piece.PieceType.Pawn:
                            var (pawnCanMove, pawnSpot) = checkPawnMoves(player, start, board, piece);
                            if (pawnCanMove)
                            {
                                return (true, pawnSpot);
                            }
                            break;

                        case Piece.PieceType.Knight:
                            var (knightCanMove, knightSpot) = checkKnightMoves(player, start, board);
                            if (knightCanMove)
                            {
                                return (true, knightSpot);
                            }
                            break;

                        case Piece.PieceType.Bishop:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                var (bishopCanMove, bishopSpot) = checkBishopMoves(player, start, board, direction);
                                if (bishopCanMove)
                                {
                                    return (true, bishopSpot);
                                }
                            }
                            break;

                        case Piece.PieceType.Queen:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                var (rookCanMove, rookSpot) = checkRookMoves(player, direction, board, start);
                                var (bishopCanMove, bishopSpot) = checkBishopMoves(player, start, board, direction);
                                if (rookCanMove)
                                {
                                    return (true, rookSpot);
                                }
                                if (bishopCanMove)
                                {
                                    return (true, bishopSpot);
                                }
                            }
                            break;

                        case Piece.PieceType.King:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                var (kingCanMove, kingSpot) = checkKingMoves(player, start, board, direction);
                                if (kingCanMove)
                                {
                                    return (true, start);
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                Debug.WriteLine($"Unable to determine if legal moves are possible. Fatal Error Occurred. {e}");
            }
            return (false, null);
        }

        private (bool, Spot) checkRookMoves(Player player, Piece.Direction direction, Board board, Spot start)
        {
            bool isWhite = player.IsWhite;
            int rowDelta = 0, colDelta = 0;
            switch (direction)
            {
                case Piece.Direction.North: rowDelta = -1; break;
                case Piece.Direction.South: rowDelta = 1; break;
                case Piece.Direction.East: colDelta = 1; break;
                case Piece.Direction.West: colDelta = -1; break;
                default: return (false, null);
            }

            int pieceRow = start.Row + rowDelta;
            int pieceCol = start.Column + colDelta;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.GetSpot(pieceRow, pieceCol);
                Piece currentPiece = currentSpot.Piece;

                if (currentPiece != null)
                {
                    if (currentPiece.isWhite() != isWhite)
                    {
                        if (TryMoveRook(start, currentSpot, board, player))
                        {
                            return (true, currentSpot);
                        }
                    }
                    break;
                }
                else
                {
                    if (TryMoveRook(start, currentSpot, board, player))
                    {
                        return (true, currentSpot);
                    }
                }

                pieceRow += rowDelta;
                pieceCol += colDelta;
            }

            return (false, null);
        }

        private bool TryMoveRook(Spot start, Spot target, Board board, Player player)
        {
            bool isWhite = player.IsWhite;

            Piece movedPiece = start.Piece;
            Piece capturedPiece = target.Piece;

            gameRules.RemoveActivePiece(capturedPiece);

            target.Piece = movedPiece;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            board.UpdateThreatMap(gameRules.GetActivePieces(isWhite));

            bool kingSafe = !board.IsKingInCheck(isWhite);

            start.Piece = movedPiece;
            target.Piece = capturedPiece;
            gameRules.AddActivePiece(capturedPiece);

            return kingSafe;
        }


        private (bool, Spot) checkPawnMoves(Player player, Spot start, Board board, Piece pawn)
        {
            int row = start.Row;
            int col = start.Column;
            bool isWhite = player.IsWhite;
            int direction = isWhite ? -1 : 1;

            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int newCol = col + colOffset;
                if (newCol >= 0 && newCol < 8)
                {
                    Spot attackSpot = board.GetSpot(row + direction, newCol);
                    Piece attackedPiece = attackSpot.Piece;
                    if (attackedPiece != null && attackedPiece.isWhite() != isWhite)
                    {
                        attackSpot.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.IsKingInCheck(isWhite))
                        {
                            start.Piece = pawn;
                            attackSpot.Piece = attackedPiece;
                            return (true, attackSpot);
                        }

                        start.Piece = pawn;
                        attackSpot.Piece = attackedPiece;
                    }
                }
            }

            if (row + direction >= 0 && row + direction < 8)
            {
                Spot moveSpot = board.GetSpot(row + direction, col);
                if (moveSpot.Piece == null)
                {
                    moveSpot.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                    if (!board.IsKingInCheck(isWhite))
                    {
                        start.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        moveSpot.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        return (true, moveSpot);
                    }

                    start.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    moveSpot.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
            }

            if ((isWhite && row == 6) || (!isWhite && row == 1))
            {
                int doubleMoveRow = row + 2 * direction;
                if (doubleMoveRow >= 0 && doubleMoveRow < 8)
                {
                    Spot doubleMoveSpot = board.GetSpot(doubleMoveRow, col);
                    if (doubleMoveSpot.Piece == null)
                    {
                        doubleMoveSpot.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.IsKingInCheck(isWhite))
                        {
                            start.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            doubleMoveSpot.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            return (true, doubleMoveSpot);
                        }

                        start.Piece = pawn;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        doubleMoveSpot.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    }
                }
            }

            return (false, null);
        }

        private (bool, Spot) checkKnightMoves(Player player, Spot start, Board board)
        {
            bool isWhite = player.IsWhite;
            int[] rowOffsets = { 2, 2, 1, -1, -2, -2, 1, -1 };
            int[] colOffsets = { -1, 1, -2, -2, -1, 1, 2, 2 };

            for (int i = 0; i < rowOffsets.Length; i++)
            {
                int newRow = start.Row + rowOffsets[i];
                int newCol = start.Column + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    Spot moveSpot = board.GetSpot(newRow, newCol);
                    Piece targetPiece = moveSpot.Piece;

                    if (targetPiece == null || targetPiece.isWhite() != isWhite)
                    {
                        Piece capturedPiece = targetPiece;
                        moveSpot.Piece = start.Piece;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.IsKingInCheck(isWhite))
                        {
                            start.Piece = moveSpot.Piece;
                            moveSpot.Piece = capturedPiece;
                            return (true, moveSpot);
                        }

                        start.Piece = moveSpot.Piece;
                        moveSpot.Piece = capturedPiece;
                    }
                }
            }

            return (false, null);
        }

        private (bool, Spot) checkBishopMoves(Player player, Spot start, Board board, Piece.Direction direction)
        {
            bool isWhite = player.IsWhite;
            int rows = 0;
            int cols = 0;
            Piece currentPiece = start.Piece;

            switch (direction)
            {
                case Piece.Direction.Northeast:
                    rows = -1; cols = 1;
                    break;
                case Piece.Direction.Northwest:
                    rows = -1; cols = -1;
                    break;
                case Piece.Direction.Southeast:
                    rows = 1; cols = 1;
                    break;
                case Piece.Direction.Southwest:
                    rows = 1; cols = -1;
                    break;
                default:
                    return (false, null);
            }

            int pieceRow = start.Row + rows;
            int pieceCol = start.Column + cols;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.GetSpot(pieceRow, pieceCol);
                Piece pieceAtSpot = currentSpot.Piece;

                if (pieceAtSpot != null)
                {
                    if (pieceAtSpot.isWhite() != isWhite)
                    {
                        if (TryMoveBishop(start, currentSpot, board, player))
                        {
                            return (true, currentSpot);
                        }
                    }
                    break;
                }
                else
                {
                    if (TryMoveBishop(start, currentSpot, board, player))
                    {
                        return (true, currentSpot);
                    }
                }

                pieceRow += rows;
                pieceCol += cols;
            }

            return (false, null);
        }

        private bool TryMoveBishop(Spot start, Spot target, Board board, Player player)
        {
            bool isWhite = player.IsWhite;
            Piece movedPiece = start.Piece;
            Piece capturedPiece = target.Piece;

            target.Piece = movedPiece;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            start.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            bool kingSafe = !board.IsKingInCheck(isWhite);

            start.Piece = movedPiece;
            target.Piece = capturedPiece;

            return kingSafe;
        }

        private (bool, Spot) checkKingMoves(Player player, Spot start, Board board, Piece.Direction direction)
        {
            int rowDelta = 0;
            int colDelta = 0;
            Piece currentPiece = start.Piece;
            bool isWhite = player.IsWhite;

            switch (direction)
            {
                case Piece.Direction.North: rowDelta = -1; break;
                case Piece.Direction.South: rowDelta = 1; break;
                case Piece.Direction.East: colDelta = 1; break;
                case Piece.Direction.West: colDelta = -1; break;
                case Piece.Direction.Northeast: rowDelta = -1; colDelta = 1; break;
                case Piece.Direction.Northwest: rowDelta = -1; colDelta = -1; break;
                case Piece.Direction.Southeast: rowDelta = 1; colDelta = 1; break;
                case Piece.Direction.Southwest: rowDelta = 1; colDelta = -1; break;
                default: return (false, null);
            }

            int newRow = start.Row + rowDelta;
            int newCol = start.Column + colDelta;

            if (Math.Abs(newRow - start.Row) <= 1 && Math.Abs(newCol - start.Column) <= 1)
            {
                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    Spot moveSpot = board.GetSpot(newRow, newCol);

                    // Store the original state
                    Spot originalSpot = start;
                    Piece originalTargetPiece = moveSpot.Piece;

                    // Temporarily move the king
                    moveSpot.Piece = currentPiece;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    originalSpot.Piece = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                    currentPiece.setCurrentPosition(moveSpot);

                    // Temporarily set the captured piece's position to null if there is one
                    if (originalTargetPiece != null)
                    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        originalTargetPiece.setCurrentPosition(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        player.removePiece(originalTargetPiece);
                    }

                    // Update the threat map and check if the king is in check
                    board.UpdateThreatMap(gameRules.GetActivePieces(isWhite));
                    bool isMoveSafe = !board.IsKingInCheck(isWhite);

                    // Revert the move
                    if (originalTargetPiece != null)
                    {
                        moveSpot.Piece = originalTargetPiece; // Restore the captured piece, if any
                        originalTargetPiece.setCurrentPosition(moveSpot);
                        player.addPiece(originalTargetPiece);
                    }

                    originalSpot.Piece = currentPiece;
                    currentPiece.setCurrentPosition(originalSpot);

                    if (isMoveSafe)
                    {
                        return (true, moveSpot);
                    }
                }
            }

            return (false, null);
        }




    }
}
