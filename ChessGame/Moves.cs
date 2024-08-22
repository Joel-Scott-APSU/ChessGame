using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using static ChessGame.Piece;

namespace ChessGame
{
    public class Moves
    {
        private Player whitePlayer;
        private Player blackPlayer;
        public Moves(Player blackPlayer, Player whitePlayer)
        {
            this.blackPlayer = blackPlayer;
            this.whitePlayer = whitePlayer;
        }

        public bool checkForLegalMoves(bool isWhite, Board board)
        {
            try
            {
                List<Piece> pieces = isWhite ? whitePlayer.getPieces() : blackPlayer.getPieces();

                foreach (Piece piece in pieces)
                {
                    Spot start = piece.getCurrentPosition();

                    switch (piece.type)
                    {
                        case Piece.PieceType.Rook:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                if(checkRookMoves(isWhite, direction, board, start))
                                {
                                    return true;
                                }
                            }
                            break;

                        case Piece.PieceType.Pawn:
                            if(checkPawnMoves(isWhite, start, board, piece))
                            {
                                return true;
                            }
                            break;

                        case Piece.PieceType.Knight:
                            if(checkKnightMoves(isWhite, start, board))
                            {
                                return true;
                            }
                            break;

                        case Piece.PieceType.Bishop:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                if (checkBishopMoves(isWhite, start, board, direction))
                                {
                                    return true;
                                }
                            }
                            break;

                        case Piece.PieceType.Queen:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                if(checkRookMoves(isWhite, direction, board, start) ||
                                checkBishopMoves(isWhite, start, board, direction))
                                {
                                    return true;
                                }
                            }
                            break;

                        case Piece.PieceType.King:
                            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
                            {
                                if(checkKingMoves(isWhite, start, board, direction))
                                {
                                    return true;
                                }
                            }
                            break;
                    }
                }
            }catch(Exception e)
            {
                Debug.WriteLine(e.ToString());
                Debug.WriteLine($"Unable to determine if legal moves are possible. Fatal Error Occurred. {e}");
            }
            return false;
        }

        private bool checkRookMoves(bool isWhite, Piece.Direction direction, Board board, Spot start)
        {
            int rowDelta = 0, colDelta = 0;
            switch (direction)
            {
                case Piece.Direction.North: rowDelta = -1; break;
                case Piece.Direction.South: rowDelta = 1; break;
                case Piece.Direction.East: colDelta = 1; break;
                case Piece.Direction.West: colDelta = -1; break;
                default: return false; // If an invalid direction is provided, return false
            }

            int pieceRow = start.GetRow() + rowDelta;
            int pieceCol = start.GetColumn() + colDelta;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8) // While the move is within the board
            {
                Spot currentSpot = board.getSpot(pieceRow, pieceCol); // Get the current spot on the board
                Piece currentPiece = currentSpot.GetPiece(); // Get the piece at the current spot

                if (currentPiece != null) // If there's a piece on the current spot
                {
                    if (currentPiece.isWhite() != isWhite) // If the piece is of the opposite color
                    {
                        if (TryMoveRook(start, currentSpot, board, isWhite)) // Try moving the rook to see if it resolves the check
                        {
                            return true;
                        }
                    }
                    break; // Stop checking further in this direction if a piece is encountered
                }
                else // If the current spot is empty
                {
                    if (TryMoveRook(start, currentSpot, board, isWhite)) // Try moving the rook to see if it resolves the check
                    {
                        return true;
                    }
                }

                pieceRow += rowDelta;
                pieceCol += colDelta;
            }

            return false; // Return false if no legal move was found to take the king out of check
        }

        private bool TryMoveRook(Spot start, Spot target, Board board, bool isWhite)
        {
            Piece movedPiece = start.GetPiece(); // Temporarily hold the piece to move
            Piece capturedPiece = target.GetPiece(); // Temporarily hold the piece being captured (if any)

            target.SetPiece(movedPiece); // Move the rook to the target spot
            start.SetPiece(null); // Clear the start spot

            bool kingSafe = !board.isKingInCheck(isWhite); // Check if the move resolves the check on the king

            // Restore the original board state
            start.SetPiece(movedPiece);
            target.SetPiece(capturedPiece);

            return kingSafe; // Return whether the move took the king out of check
        }


        private bool checkPawnMoves(bool isWhite, Spot start, Board board, Piece pawn)
        {
            int row = start.GetRow();
            int col = start.GetColumn();
            int direction = isWhite ? -1 : 1;
            Piece piece = start.GetPiece();

            // Capture diagonally
            for (int colOffset = -1; colOffset <= 1; colOffset += 2)
            {
                int newCol = col + colOffset;
                if (newCol >= 0 && newCol < 8)
                {
                    Spot attackSpot = board.getSpot(row + direction, newCol);
                    Piece attackedPiece = attackSpot.GetPiece();
                    if (attackedPiece != null && attackedPiece.isWhite() != isWhite)
                    {
                        attackSpot.SetPiece(piece);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(piece);
                            attackSpot.SetPiece(attackedPiece);
                            return true;
                        }

                        start.SetPiece(piece);
                        attackSpot.SetPiece(attackedPiece);
                    }
                }
            }

            // Move forward
            if (row + direction >= 0 && row + direction < 8)
            {
                Spot moveSpot = board.getSpot(row + direction, col);
                if (moveSpot.GetPiece() == null)
                {
                    moveSpot.SetPiece(piece);
                    start.SetPiece(null);

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(piece);
                        moveSpot.SetPiece(null);
                        return true;
                    }

                    start.SetPiece(piece);
                    moveSpot.SetPiece(null);
                }
            }

            // Special handling for double move from starting row
            if ((isWhite && row == 6) || (!isWhite && row == 1))
            {
                int doubleMoveRow = row + 2 * direction;
                if (doubleMoveRow >= 0 && doubleMoveRow < 8)
                {
                    Spot doubleMoveSpot = board.getSpot(doubleMoveRow, col);
                    if (doubleMoveSpot.GetPiece() == null)
                    {
                        doubleMoveSpot.SetPiece(piece);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(piece);
                            doubleMoveSpot.SetPiece(null);
                            return true;
                        }

                        start.SetPiece(piece);
                        doubleMoveSpot.SetPiece(null);
                    }
                }
            }

            return false;
        }


        private bool checkKnightMoves(bool isWhite, Spot start, Board board)
        {
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
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(moveSpot.GetPiece());
                            moveSpot.SetPiece(capturedPiece);
                            return true;
                        }

                        start.SetPiece(moveSpot.GetPiece());
                        moveSpot.SetPiece(capturedPiece);
                    }
                }
            }

            return false;
        }


        private bool checkBishopMoves(bool isWhite, Spot start, Board board, Piece.Direction direction)
        {
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
            }

            int pieceRow = start.GetRow() + rows;
            int pieceCol = start.GetColumn() + cols;

            while(pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.getSpot(pieceRow, pieceCol);
                Piece capturedPiece = currentSpot.GetPiece();


                if (currentSpot.GetPiece() != null)
                {
                    if(currentSpot.GetPiece().isWhite() != isWhite)
                    {
                        currentSpot.SetPiece(currentPiece);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(currentPiece);
                            currentSpot.SetPiece(capturedPiece);
                            return true;
                        }

                        start.SetPiece(currentPiece);
                        currentSpot.SetPiece(capturedPiece);
                    }
                }

                else
                {
                    currentSpot.SetPiece(currentPiece);
                    start.SetPiece(null);

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(currentPiece);
                        currentSpot.SetPiece(capturedPiece);
                        return true;
                    }

                    start.SetPiece(currentPiece);
                    currentSpot.SetPiece(capturedPiece);
                }

                pieceRow += rows;
                pieceCol += cols;
            }

            return false;
        }

        private bool checkKingMoves(bool isWhite, Spot start, Board board, Piece.Direction direction)
        {

            Piece king = start.GetPiece();
            int rows = 0;
            int cols = 0;

            switch (direction)
            {
                case Piece.Direction.North:
                    rows = -1;
                    break;
                case Piece.Direction.South:
                    rows = 1;
                    break;
                case Piece.Direction.East:
                    cols = 1;
                    break;
                case Piece.Direction.West:
                    cols = -1;
                    break;
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
            }

            int pieceRow = start.GetRow() + rows;
            int pieceCol = start.GetColumn() + cols;

            while (pieceRow >= 0 && pieceRow < 8 && pieceCol >= 0 && pieceCol < 8)
            {
                Spot currentSpot = board.getSpot(pieceRow, pieceCol);
                Piece capturedPiece = currentSpot.GetPiece();

                if(currentSpot.GetPiece() != null)
                {
                    if(capturedPiece.isWhite() != isWhite)
                    {
                        currentSpot.SetPiece(king);
                        start.SetPiece(null);

                        if (!board.isKingInCheck(isWhite))
                        {
                            start.SetPiece(king);
                            currentSpot.SetPiece(capturedPiece);
                            return true;
                        }

                        start.SetPiece(king);
                        currentSpot.SetPiece(capturedPiece);
                    }
                }
                else
                {
                    currentSpot.SetPiece(king);
                    start.SetPiece(null);

                    if (!board.isKingInCheck(isWhite))
                    {
                        start.SetPiece(king);
                        currentSpot.SetPiece(capturedPiece);
                        return true;
                    }

                    start.SetPiece(king);
                    currentSpot.SetPiece(capturedPiece);
                   
                }
            }
            return false;
        }
    }
}
