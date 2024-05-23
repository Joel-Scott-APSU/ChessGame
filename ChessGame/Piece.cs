using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Navigation;

namespace ChessGame
{
    internal abstract class Piece
    {
        private bool taken = false;
        private bool white = false;

        public Piece(bool white)
        {
            this.setWhite(white);
        }

        public bool isWhite() { return this.white; }

        public void setWhite(bool white) { this.white = white; }

        public bool isTaken() { return this.taken; }

        public void setTaken(bool taken) { this.taken = taken; }

        public abstract bool legalMove(Board board, Spot start, Spot end);

        public class King : Piece
        {
            public bool castling = false;
            public King(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                //Check to see if the spot where we are trying to move to is already occupied by a piece of that color
                if (end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                int x = Math.Abs(start.getX() - end.getX());
                int y = Math.Abs(start.getY() - end.getY());
                if (x + y == 1)
                {
                    return true;
                }
                return false;
            }
        }

        public class Pawn : Piece
        {
            public Pawn(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int x = end.getX() - start.getX();
                int y = Math.Abs(end.getY() - start.getY());
                int z = start.getX() - end.getX();
                Spot adjacentLeft = board.getSpot(start.getX(), start.getY() - 1);
                Spot adjacentRight = board.getSpot(start.getX(), start.getY() + 1);

                //checks if the pawn is its starting position 
                bool isStartingPosition = (this.isWhite() && start.getX() == 1) || (!this.isWhite() && start.getX() == 6);
                //checks to see if the piece is white
                if (isWhite())
                {
                    if (x == 0 && y == 0)
                    {
                        return false;
                    }
                    else if (end.getX() == 7)
                    {

                        return true;
                    }
                    //checks if the move the user is attempting to make is a valid move forward, barring the pawns first move  
                    else if ((x == 1 || x == 2 && isStartingPosition) && y == 0 && end.GetPiece() == null)
                    {
                        return true;
                    }
                    //checks if the piece is able to capture another piece is a diagonal forward square 
                    else if (x == 1 && y == 1 && end.GetPiece() != null && !end.GetPiece().isWhite())
                    {
                        return true;
                    }
                }

                else if (!isWhite())
                {
                    if (x == 0 && y == 0)
                    {
                        return false;
                    }
                    else if (end.getX() == 0)
                    {
                        return true;
                    }
                    else if ((z == 1 || z == 2 && isStartingPosition) && y == 0 && end.GetPiece == null)
                    {
                        return true;
                    }
                    else if (z == 1 && y == 1 && end.GetPiece != null && end.GetPiece().isWhite())
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public class Rook : Piece
        {
            public Rook(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int x = start.getX() - end.getX();
                int y = start.getY() - end.getY();

                if (isWhite())
                {
                    if ((x == 0 && y == 0) || (x != 0 && y != 0))
                    {
                        return false;
                    }
                    else if (x < 0)
                    {
                        for (int i = start.getX() + 1; i < end.getX() - 1; i++)
                        {
                            if (board.getSpot(i, start.getY()).GetPiece() != null)
                            {
                                return false;
                            }
                        }
                        //initialize function in move class to destroy piece at existing square when you move and call it here before returning true 
                        //initialize function to check if moving piece puts own king in check
                        return true;
                    }

                    else if (x > 0)
                    {
                        for (int i = start.getX() - 1; i > start.getX() + 1; i--)
                        {
                            if (board.getSpot(i, start.getY()).GetPiece() != null)
                            {
                                return false;
                            }
                        }
                        //initialize function in move class to destroy piece at existing square when you move and call it here before returning true 
                        //initialize function to check if moving piece puts own king in check
                        return true;
                    }

                    else if (y < 0)
                    {
                        for (int i = start.getY() + 1; i < start.getY() - 1; i++)
                        {
                            if (board.getSpot(i, start.getX()).GetPiece() != null)
                            {
                                return false;
                            }
                        }
                        //initialize function in move class to destroy piece at existing square when you move and call it here before returning true 
                        //initialize function to check if moving piece puts own king in check
                        return true;
                    }

                    else if (y > 0)
                    {
                        for (int i = start.getY() - 1; i > start.getY() + 1; i--)
                        {
                            if (board.getSpot(i, start.getX()).GetPiece() != null)
                            {
                                return false;
                            }
                        }
                        //initialize function in move class to destroy piece at existing square when you move and call it here before returning true 
                        //initialize function to check if moving piece puts own king in check
                        return true;
                    }
                }
                return false;
            }

        }

        public class Knight : Piece
        {
            public Knight(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                return false;
            }
        }

        public class Bishop : Piece
        {
            public Bishop(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                return false;
            }
        }

        public class Queen : Piece
        {
            public Queen(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                return false;
            }
        }
    }
}
