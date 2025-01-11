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

        public Moves(Player whitePlayer, Player blackPlayer)
        {
            this.blackPlayer = blackPlayer;
            this.whitePlayer = whitePlayer;
        }

        public (bool, Spot) checkForLegalMoves(Player player, Board board, List<Piece> pieces)
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

            int pieceRow = start.GetRow() + rowDelta;
            int pieceCol = start.GetColumn() + colDelta;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.getSpot(pieceRow, pieceCol);
                Piece currentPiece = currentSpot.GetPiece();

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
            List<Piece> pieces = isWhite ? whitePlayer.getPieces() : blackPlayer.getPieces();

            Piece movedPiece = start.GetPiece();
            Piece capturedPiece = target.GetPiece();

            pieces.Remove(capturedPiece);

            target.SetPiece(movedPiece);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            board.updateThreatMap(pieces);

            bool kingSafe = !board.isKingInCheck(isWhite);

            start.SetPiece(movedPiece);
            target.SetPiece(capturedPiece);
            pieces.Add(capturedPiece);

            return kingSafe;
        }

        private (bool, Spot) checkPawnMoves(Player player, Spot start, Board board, Piece pawn)
        {
            int row = start.GetRow();
            int col = start.GetColumn();
            bool isWhite = player.IsWhite;
            int direction = isWhite ? -1 : 1;

            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int newCol = col + colOffset;
                if (newCol >= 0 && newCol < 8)
                {
                    Spot attackSpot = board.getSpot(row + direction, newCol);
                    Piece attackedPiece = attackSpot.GetPiece();
                    if (attackedPiece != null && attackedPiece.isWhite() != isWhite)
                    {
                        attackSpot.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(pawn);
                            attackSpot.SetPiece(attackedPiece);
                            return (true, attackSpot);
                        }

                        start.SetPiece(pawn);
                        attackSpot.SetPiece(attackedPiece);
                    }
                }
            }

            if (row + direction >= 0 && row + direction < 8)
            {
                Spot moveSpot = board.getSpot(row + direction, col);
                if (moveSpot.GetPiece() == null)
                {
                    moveSpot.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        moveSpot.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                        return (true, moveSpot);
                    }

                    start.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    moveSpot.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
            }

            if ((isWhite && row == 6) || (!isWhite && row == 1))
            {
                int doubleMoveRow = row + 2 * direction;
                if (doubleMoveRow >= 0 && doubleMoveRow < 8)
                {
                    Spot doubleMoveSpot = board.getSpot(doubleMoveRow, col);
                    if (doubleMoveSpot.GetPiece() == null)
                    {
                        doubleMoveSpot.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                            doubleMoveSpot.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                            return (true, doubleMoveSpot);
                        }

                        start.SetPiece(pawn);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        doubleMoveSpot.SetPiece(null);
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
                int newRow = start.GetRow() + rowOffsets[i];
                int newCol = start.GetColumn() + colOffsets[i];

                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    Spot moveSpot = board.getSpot(newRow, newCol);
                    Piece targetPiece = moveSpot.GetPiece();

                    if (targetPiece == null || targetPiece.isWhite() != isWhite)
                    {
                        Piece capturedPiece = targetPiece;
                        moveSpot.SetPiece(start.GetPiece());
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                        start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(moveSpot.GetPiece());
                            moveSpot.SetPiece(capturedPiece);
                            return (true, moveSpot);
                        }

                        start.SetPiece(moveSpot.GetPiece());
                        moveSpot.SetPiece(capturedPiece);
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
            Piece currentPiece = start.GetPiece();

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

            int pieceRow = start.GetRow() + rows;
            int pieceCol = start.GetColumn() + cols;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.getSpot(pieceRow, pieceCol);
                Piece pieceAtSpot = currentSpot.GetPiece();

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
            Piece movedPiece = start.GetPiece();
            Piece capturedPiece = target.GetPiece();

            target.SetPiece(movedPiece);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            start.SetPiece(null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            bool kingSafe = !board.isKingInCheck(isWhite);

            start.SetPiece(movedPiece);
            target.SetPiece(capturedPiece);

            return kingSafe;
        }

        private (bool, Spot) checkKingMoves(Player player, Spot start, Board board, Piece.Direction direction)
        {
            int rowDelta = 0;
            int colDelta = 0;
            Piece currentPiece = start.GetPiece();
            List<Piece> pieces = player.getPieces();
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

            int newRow = start.GetRow() + rowDelta;
            int newCol = start.GetColumn() + colDelta;

            if (Math.Abs(newRow - start.GetRow()) <= 1 && Math.Abs(newCol - start.GetColumn()) <= 1)
            {
                if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
                {
                    Spot moveSpot = board.getSpot(newRow, newCol);

                    // Store the original state
                    Spot originalSpot = start;
                    Piece originalTargetPiece = moveSpot.GetPiece();

                    // Temporarily move the king
                    moveSpot.SetPiece(currentPiece);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    originalSpot.SetPiece(null);
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
                    board.updateThreatMap(pieces);
                    bool isMoveSafe = !board.isKingInCheck(isWhite);

                    // Revert the move
                    if (originalTargetPiece != null)
                    {
                        moveSpot.SetPiece(originalTargetPiece); // Restore the captured piece, if any
                        originalTargetPiece.setCurrentPosition(moveSpot);
                        player.addPiece(originalTargetPiece);
                    }

                    originalSpot.SetPiece(currentPiece);
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
