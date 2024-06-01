using System;
using System.Collections.Generic;
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

                if(board.isKingInCheck(isWhite)) return false;
                
                if(kingSpot.GetPiece() is King king && rookSpot.GetPiece() is Rook rook)
                {
                    if(king.hasMoved || rook.HasMoved)
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

                if(kingSpot.GetPiece() is King king && rookSpot.GetPiece() is Rook rook)
                {
                    if (king.hasMoved || rook.HasMoved) return false;

                    for(int i = 3; i > 1; i--)
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
            public Pawn(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                int x = Math.Abs(start.getX() - end.getX());
                int y = Math.Abs(start.getY() - end.getY());

                int startPoint = isWhite() ? 1 : 6;


                if ((start.getY() != startPoint && y == 2) ||
                    (y != 1 || x != 0) ||
                    (!enPassant()) ||
                    !canPawnAttack(board, start, end))
                {
                    return false;
                }

                return true;
            }

            public bool enPassant()
            {
                return true;
            }

            public bool canPawnAttack(Board board, Spot start, Spot end)
            {
                int direction = isWhite() ? 1 : -1;
                int y = (end.getY() - start.getY());
                int x = Math.Abs(start.getX() - end.getX());
                return (direction == y && x == 1 && end.GetPiece() != null && end.GetPiece().isWhite() != this.isWhite());
            }
        }

        public class Rook : Piece
        {
            private bool hasMoved = false;

            public bool HasMoved { get { return hasMoved; } }

            public Rook(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startX = start.getX();
                int startY = start.getY();
                int endX = end.getX();
                int endY = end.getY();

                //spot is occupied by a piece of the same color 
                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (!legalRookMove(board, startX, startY, endX, endY))
                {
                    return false;
                }

                if(hasMoved == false)
                {
                    hasMoved = true;
                }

                return true;
            }

            public static bool legalRookMove(Board board, int startX, int startY, int endX, int endY)
            {
                int movementDirectionX = (endX - startX) == 0 ? 0 : (endX - startX) / Math.Abs(endX - startX);
                int movementDirectionY = (endY - startY) == 0 ? 0 : (endY - startY) / Math.Abs(endY - startY);

                if (movementDirectionX != 0 && movementDirectionY != 0)
                {
                    return false;
                }

                int currentX = startX + movementDirectionX;
                int currentY = startY + movementDirectionY;

                while (currentX != endX || currentY != endY)
                {
                    if (board.getSpot(currentX, currentY).GetPiece() != null)
                    {
                        return false;
                    }

                    currentX += movementDirectionX;
                    currentY += movementDirectionY;
                }

                return true;
            }

        }

        public class Knight : Piece
        {
            public Knight(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {

                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                int x = Math.Abs(start.getX() - end.getX());
                int y = Math.Abs(start.getY() - end.getY());

                if ((x * y) != 2)
                {
                    return false;
                }


                return true;
            }

        }

        public class Bishop : Piece
        {
            public Bishop(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startX = start.getX();
                int startY = start.getY();
                int endX = end.getX();
                int endY = end.getY();

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (startX == endX || startY == endY)
                {
                    return false;
                }

                if (!legalBishopMove(board, startX, startY, endX, endY))
                {
                    return false;
                }

                return true;
            }

            public static bool legalBishopMove(Board board, int startX, int startY, int endX, int endY)
            {
                int movementDirectionX = Math.Sign(endX - startX);
                int MovementDirectionY = Math.Sign(endY - startY);

                if (Math.Abs(endX - startX) != Math.Abs(endY - startY))
                {
                    return false;
                }

                int currentX = startX + movementDirectionX;
                int currentY = startY + MovementDirectionY;

                while (currentX != endX && currentY != endY)
                {
                    if (board.getSpot(currentX, currentY).GetPiece() != null)
                    {
                        return false;
                    }

                    currentX += movementDirectionX;
                    currentY += MovementDirectionY;
                }

                return true;
            }
        }

        public class Queen : Piece
        {
            public Queen(bool white) : base(white) { }

            override
            public bool legalMove(Board board, Spot start, Spot end)
            {
                int startX = start.getX();
                int startY = start.getY();
                int endX = end.getX();
                int endY = end.getY();

                if (board.isKingInCheck(isWhite()))
                {
                    return false;
                }

                if (end.GetPiece() != null && end.GetPiece().isWhite() == this.isWhite())
                {
                    return false;
                }

                if (!Rook.legalRookMove(board, startX, startY, endX, endY) && !Bishop.legalBishopMove(board, startX, startY, endX, endY))
                {
                    return false;
                }

                return true;
            }


        }
    }
}
