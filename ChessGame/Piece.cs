using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Printing;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Navigation;

namespace ChessGame
{
    public abstract class Piece
    {
        private bool taken = false;
        private bool white = false;

        private ChessBoardSquare selectedSquare;

        public Piece(bool white)
        {
            this.setWhite(white);
        }

        public bool isWhite() { return this.white; }

        public void setWhite(bool white) { this.white = white; }

        public bool isTaken() { return this.taken; }

        public void setTaken(bool taken) { this.taken = taken; }

        public abstract bool legalMove(Board board, Spot start, Spot end);

        public enum PieceType
        {
            Pawn,
            Knight,
            Bishop,
            Queen,
            Rook,
            King
        }

        public enum Direction
        {
            North,
            South,
            East,
            West,
            Northeast,
            Northwest,
            Southeast,
            Southwest
        }

        public PieceType type { get; set; }

        public override string ToString()
        {
            return $"{(isWhite() ? "White" : "Black")} {type}";
        }
        public class King : Piece
        {
            public bool castling = false;

            private bool hasMoved = false;
            public King(bool white) : base(white)
            {
                this.type = PieceType.King;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                //Check to see if the spot where we are trying to move to is already occupied by a piece of that color
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }


                return true;
            }

            private bool canCastleKingside(bool isWhite, Board board)
            {
                int row = isWhite ? 0 : 7;

                Spot rookSpot = board.getSpot(row, 7);
                Spot kingSpot = board.getSpot(row, 4);

                if (kingSpot == null || rookSpot == null) return false;

                if (board.isKingInCheck(isWhite)) return false;

                if (kingSpot.GetPiece() is King king && rookSpot.GetPiece() is Rook rook)
                {
                    if (king.hasMoved || rook.HasMoved)
                    {
                        return false;
                    }

                    for (int i = 5; i <= 6; i++)
                    {
                        if (board.isSquareUnderThreat(isWhite, i, row) || board.getSpot(i, row).GetPiece() != null) return false;
                    }
                }
                else
                {
                    return false;
                }

                return true;
            }

            private bool canCastleQueenside(bool isWhite, Board board)
            {
                int row = isWhite ? 0 : 7;

                Spot rookSpot = board.getSpot(row, 0);
                Spot kingSpot = board.getSpot(row, 4);

                if (kingSpot == null || rookSpot == null) return false;

                if (board.isKingInCheck(isWhite)) return false;

                if (kingSpot.GetPiece() is King king && rookSpot.GetPiece() is Rook rook)
                {
                    if (king.hasMoved || rook.HasMoved) return false;

                    for (int i = 3; i > 1; i--)
                    {
                        if (board.isSquareUnderThreat(isWhite, row, i) || board.getSpot(row, i).GetPiece() != null) return false;
                    }

                    if (board.getSpot(row, 1).GetPiece() != null) return false;
                }
                else
                {
                    return false;
                }
                return true;
            }
        }

        public class Pawn : Piece
        {
            public Pawn(bool white) : base(white)
            {
                this.type = PieceType.Pawn;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    Debug.WriteLine("Piece of the same color on the spot");
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    Debug.WriteLine("Move puts your own king in check");
                    return false;
                }

                int rowDifference = end.GetRow() - start.GetRow();
                int columnDifference = Math.Abs(start.GetColumn() - end.GetColumn());

                // White pawn moves
                if (isWhite())
                {
                    // Forward move by 1
                    if (rowDifference == -1 && columnDifference == 0 && end.GetPiece() == null)
                    {
                        return true;
                    }
                    // Forward move by 2 from the starting position
                    if (start.GetRow() == 6 && rowDifference == -2 && columnDifference == 0 && end.GetPiece() == null)
                    {
                        return true;
                    }
                    // Capture move
                    if (rowDifference == -1 && columnDifference == 1 && end.GetPiece() != null && !end.GetPiece().isWhite())
                    {
                        return true;
                    }
                }

                // Black pawn moves
                else
                {
                    // Forward move by 1
                    if (rowDifference == 1 && columnDifference == 0 && end.GetPiece() == null)
                    {
                        return true;
                    }
                    // Forward move by 2 from the starting position
                    if (start.GetRow() == 1 && rowDifference == 2 && columnDifference == 0 && end.GetPiece() == null)
                    {
                        return true;
                    }
                    // Capture move
                    if (rowDifference == 1 && columnDifference == 1 && end.GetPiece() != null && end.GetPiece().isWhite())
                    {
                        return true;
                    }
                }

                Debug.WriteLine("Illegal pawn move");
                return false;
            }
        


        public bool enPassant()
            {
                return true;
            }

            public bool canPawnAttack(Board board, Spot start, Spot end)
            {
                int direction = isWhite() ? 1 : -1;
                int row = (end.GetRow() - start.GetRow());
                int column = Math.Abs(start.GetColumn() - end.GetColumn());
                return (direction == column && row == 1 && end.GetPiece() != null && end.GetPiece().isWhite() != this.isWhite());
            }
        }

        public class Rook : Piece
        {
            private bool hasMoved = false;

            public bool HasMoved { get { return hasMoved; } }

            public Rook(bool white) : base(white)
            {
                this.type = PieceType.Rook;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startRow = start.GetRow();
                int startColumn = start.GetColumn();
                int endRow = end.GetRow();
                int endColumn = end.GetColumn();

                //spot is occupied by a piece of the same color 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (!legalRookMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                if (hasMoved == false)
                {
                    hasMoved = true;
                }

                return true;
            }
            
            public static bool legalRookMove(Board board, int startRow, int startColumn, int endRow, int endColumn)
            {
                int RowMovement = (endRow - startRow) == 0 ? 0 : (endRow - startRow) / Math.Abs(endRow - startRow);
                int ColumnMovement = (endColumn - startColumn) == 0 ? 0 : (endColumn - startColumn) / Math.Abs(endColumn - startColumn);

                Debug.WriteLine($"Row Movement: {RowMovement}, Column Movement: {ColumnMovement}");
                if (RowMovement != 0 && ColumnMovement != 0)
                {
                    return false;
                }

                int currentRow = startRow + RowMovement;
                int currentColumn = startColumn + ColumnMovement;

                try
                {
                    while (currentRow != endRow || currentColumn != endColumn)
                    {
                        Debug.WriteLine($"End Row: {endRow} End Column: {endColumn}");
                        Debug.WriteLine($"Current Row: {currentRow}, Current Column: {currentColumn}");

                        if (board.getSpot(currentRow, currentColumn).GetPiece() != null)
                        {
                            return false;
                        }

                        currentRow += RowMovement;
                        currentColumn += ColumnMovement;
                    }
                }catch(ArgumentOutOfRangeException e)
                {
                    Debug.WriteLine(e);
                }

                return true;
            }

        }

        public class Knight : Piece
        {
            public Knight(bool white) : base(white)
            {
                this.type = PieceType.Knight;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                if (end != null)
                {
                    if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                    {
                        return false;
                    }
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                int x = Math.Abs(start.GetColumn() - end.GetColumn());
                int y = Math.Abs(start.GetRow() - end.GetRow());

                if ((x * y) != 2)
                {
                    return false;
                }


                return true;
            }

        }

        public class Bishop : Piece
        {
            public Bishop(bool white) : base(white)
            {
                this.type = PieceType.Bishop;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startRow = start.GetColumn();
                int startColumn = start.GetRow();
                int endRow = end.GetColumn();
                int endColumn = end.GetRow();

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (startRow == endRow || startColumn == endColumn)
                {
                    return false;
                }

                if (!legalBishopMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                return true;
            }

            public static bool legalBishopMove(Board board, int startRow, int startColumn, int endRow, int endColumn)
            {
                int RowMovementDirection = Math.Sign(endRow - startRow);
                int ColumnMovementDirection = Math.Sign(endColumn - startColumn);

                if (Math.Abs(endRow - startRow) != Math.Abs(endColumn - startColumn))
                {
                    return false;
                }

                int currentRow = startRow + RowMovementDirection;
                int currentColumn = startColumn + ColumnMovementDirection;

                while (currentRow != endRow && currentColumn != endColumn)
                {
                    if (board.getSpot(currentRow, currentColumn).GetPiece() != null)
                    {
                        return false;
                    }

                    currentRow += RowMovementDirection;
                    currentColumn += ColumnMovementDirection;
                }

                return true;
            }
        }

        public class Queen : Piece
        {
            public Queen(bool white) : base(white)
            {
                this.type = PieceType.Queen;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startRow = start.GetColumn();
                int startColumn = start.GetRow();
                int endRow = end.GetColumn();
                int endColumn = end.GetRow();

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (!Rook.legalRookMove(board, startRow, startColumn, endRow, endColumn) && !Bishop.legalBishopMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                return true;
            }


        }
    }
}
