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

        public (bool, Spot) checkForLegalMoves(Player player, Board board, IEnumerable<Piece> pieces)
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
                                Debug.WriteLine($"Can Rook move: {canMove}");
                                if (canMove)
                                {
                                    return (true, start);
                                }
                            }
                            break;

                        case Piece.PieceType.Pawn:
                            var (pawnCanMove, pawnSpot) = checkPawnMoves(player, start, board, piece);
                            Debug.WriteLine($"Can Pawn move: {pawnCanMove}");
                            if (pawnCanMove)
                            {
                                return (true, pawnSpot);
                            }
                            break;

                        case Piece.PieceType.Knight:
                            var (knightCanMove, knightSpot) = checkKnightMoves(player, start, board);
                            Debug.WriteLine($"Can Knight Move: {knightCanMove}");
                            if (knightCanMove)
                            {
                                return (true, knightSpot);
                            }
                            break;

                        case Piece.PieceType.Bishop:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                var (bishopCanMove, bishopSpot) = checkBishopMoves(player, start, board, direction);
                                Debug.WriteLine($"Can Bishop Move: {bishopCanMove}");
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
                                Debug.WriteLine($"Can queen move Horizontally/Vertically: {rookCanMove}");
                                if (rookCanMove)
                                {
                                    return (true, rookSpot);
                                }
                                Debug.WriteLine($"Can queen move Diagonally: {bishopCanMove}");
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
                                Debug.WriteLine($"Can King Move: \n{kingCanMove} Direction: {direction}");
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
            Piece StartPiece = start.Piece;
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

                if (currentPiece == null || StartPiece.isWhite() != currentPiece.isWhite())
                {
                        if (TryMoveRook(start, currentSpot, board, player))
                        {
                            Debug.WriteLine("Trying Rook move #1");
                            return (true, currentSpot);
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
            bool wasPieceCaptured = false;
            bool isMoveSafe = false; // Declare it here so it's available at the return

            Debug.WriteLine("Piece in check for legal moves: " + currentPiece);
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

            if (newRow >= 0 && newRow < 8 && newCol >= 0 && newCol < 8)
            {
                Spot moveSpot = board.GetSpot(newRow, newCol);
                Spot originalSpot = start;
                Piece originalTargetPiece = moveSpot.Piece;
                // Only proceed if the target spot is not occupied by a friendly piece
                if (originalTargetPiece == null || originalTargetPiece.isWhite() != currentPiece.isWhite())

                {
                    // Simulate the move
                    moveSpot.Piece = currentPiece;
                    originalSpot.Piece = null;
                    currentPiece.setCurrentPosition(moveSpot);

                    wasPieceCaptured = originalTargetPiece != null;
                    if (wasPieceCaptured)
                    {
                        originalTargetPiece.setCurrentPosition(null);
                        player.removePiece(originalTargetPiece);
                    }

                    Debug.WriteLine($"Updating threat map in king moves #1 Direction {direction}");
                    board.UpdateThreatMap(gameRules.GetActivePieces(!isWhite));

                    isMoveSafe = board.IsKingInCheck(isWhite);

                    // Revert the move
                    originalSpot.Piece = currentPiece;
                    currentPiece.setCurrentPosition(originalSpot);

                    if (wasPieceCaptured)
                    {
                        moveSpot.Piece = originalTargetPiece;
                        originalTargetPiece.setCurrentPosition(moveSpot);
                        player.addPiece(originalTargetPiece);
                    }
                    else
                    {
                        moveSpot.Piece = null;
                    }

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
