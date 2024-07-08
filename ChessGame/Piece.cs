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
        /***********************************
         * Pawn class that validates the 
         * movement of the pawns, including 
         * captures and en passant 
         * ********************************/
        public class Pawn : Piece
        {
            public Pawn(bool white) : base(white)
            {
                this.type = PieceType.Pawn;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                //verifies that the end square does not contain a piece of the same color 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    Debug.WriteLine("Piece of the same color on the spot");
                    return false;
                }

                //gets the column difference and row difference to verify pawn movement by color 
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

            //Checks if the pawn is able to attack based on its position and the position of pieces around it 
            public bool canPawnAttack(Board board, Spot start, Spot end)
            {
                //adjacent directions on either side of the pawn
                int direction = isWhite() ? 1 : -1;
                //row that the pawn is attempting to move to
                int row = (end.GetRow() - start.GetRow());
                //column the pawn is attempting to move to 
                int column = Math.Abs(start.GetColumn() - end.GetColumn());
                //verifies that the movement is correct and that the piece its attacking is of the opposite color 
                return (direction == column && row == 1 && end.GetPiece() != null && end.GetPiece().isWhite() != this.isWhite());
            }
        }

        /*************************************
         * Validates the movement of the rook 
         * to ensure that it only moves in a 
         * straight line and not diagonally
         * *********************************/
        public class Rook : Piece
        {
            //checks if the rook has moved for castling
            private bool hasMoved = false;

            public bool HasMoved { get { return hasMoved; } }

            public Rook(bool white) : base(white)
            {
                this.type = PieceType.Rook;
            }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                Debug.WriteLine($"Has Moved: {HasMoved}");
                //gets the starting and ending points of the rook 
                int startRow = start.GetRow();
                int startColumn = start.GetColumn();
                int endRow = end.GetRow();
                int endColumn = end.GetColumn();

                //spot is occupied by a piece of the same color 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                //validates if the rook move is legal 
                if (!legalRookMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                //if the rook has not moved, set the has moved condition to true 
                if (hasMoved == false)
                {
                    hasMoved = true;
                }

                return true;
            }

            public static bool legalRookMove(Board board, int startRow, int startColumn, int endRow, int endColumn)
            {
                //determines the movement direction of the rook
                int movementDirectionX = (endRow - startRow) == 0 ? 0 : (endRow - startRow) / Math.Abs(endRow - startRow);
                int movementDirectionY = (endColumn - startColumn) == 0 ? 0 : (endColumn - startColumn) / Math.Abs(endColumn - startColumn);

                Debug.WriteLine($"movement X: {movementDirectionX}, Y: {movementDirectionY}");
                //verifies that either the x or y direction is equal to 0
                if (movementDirectionX != 0 && movementDirectionY != 0)
                {
                    return false;
                }

                //gets the next square in the movement direction
                int currentX = startRow + movementDirectionX;
                int currentY = startColumn + movementDirectionY;
                Debug.WriteLine($"Current X: {currentX}, Current Y: {currentY}");

                //goes square by square in the movement direction to verify there are no pieces on any square in the pathway
                //goes through every square until the ending square is reached 
                while (currentX != endRow || currentY != endColumn)
                {
                    if (board.getSpot(currentX, currentY).GetPiece() != null)
                    {
                        return false;
                    }

                    //increments the movement direction to search the next square
                    currentX += movementDirectionX;
                    currentY += movementDirectionY;
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

                //checks to see if the piece at the end square is of the same color as the knight 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                //gets the row and column that the knight moves to 
                int column = Math.Abs(start.GetColumn() - end.GetColumn());
                int row = Math.Abs(start.GetRow() - end.GetRow());

                //verifies that the knight movement multiplies to 2 
                if ((column * row) != 2)
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
                //gets the start and end position of the bishop movement
                int startRow = start.GetRow();
                int startColumn = start.GetColumn();
                int endRow = end.GetRow();
                int endColumn = end.GetColumn();

                //checks to see if the square is occupied by a piece of the same color 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                //checks if the starting or ending row are the same as the one it started on 
                if (startRow == endRow || startColumn == endColumn)
                {
                    return false;
                }

                //verifies if it is a legal bishop move 
                if (!legalBishopMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                return true;
            }

            public static bool legalBishopMove(Board board, int startRow, int startColumn, int endRow, int endColumn)
            {
                //gets the direction of movement for the rows and columns 
                int RowMovementDirection = Math.Sign(endRow - startRow);
                int ColumnMovementDirection = Math.Sign(endColumn - startColumn);

                //verifies that the piece moves rows and columns 
                if (Math.Abs(endRow - startRow) != Math.Abs(endColumn - startColumn))
                {
                    return false;
                }

                //gets the row and column in the direction of movement to check for obstructions on the board
                int currentRow = startRow + RowMovementDirection;
                int currentColumn = startColumn + ColumnMovementDirection;

                //continues to check for pieces in the path of the move until the end square of movement is reached 
                //returns false if a piece is found, regardless of color 
                while (currentRow != endRow && currentColumn != endColumn)
                {
                    if (board.getSpot(currentRow, currentColumn).GetPiece() != null)
                    {
                        return false;
                    }

                    //increments the row and column to check the next square for pieces 
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
                //gets the starting and ending point for the queens move
                int startRow = start.GetColumn();
                int startColumn = start.GetRow();
                int endRow = end.GetColumn();
                int endColumn = end.GetRow();

                Debug.WriteLine($"Is Move Valid: {board.isMoveValid(start, end, isWhite())}");
                //checks that the piece on the ending square is not of the same color as the moving piece
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }


                if(!board.isMoveValid(start, end, isWhite()))
                {
                    return false;
                }

                //checks to see if the move is either a legal rook move or a legal bishop move 
                if (!Rook.legalRookMove(board, startRow, startColumn, endRow, endColumn) && !Bishop.legalBishopMove(board, startRow, startColumn, endRow, endColumn))
                {
                    return false;
                }

                return true;
            }


        }
    }
}
