using System;

namespace ChessGame.Models
{
    public class Spot
    {
        private int row;
        private int column;
        private Piece piece;

        public Spot(int row, int column, Piece piece)
        {
            this.row = row;
            this.column = column;
            this.piece = piece;
        }

        public int Row { get { return row; } }
        public int Column { get { return column; } }

        // Property to get and set the piece
        public Piece Piece
        {
            get { return piece; }
            set
            {
                // Add any logic or validation here if needed
                piece = value;
            }
        }

        public override string ToString()
        {
            string pieceString = piece != null ? piece.ToString() : "Empty";
            return $"Spot [Row={row}, Column={column}, Piece={pieceString}]";
        }
    }
}
