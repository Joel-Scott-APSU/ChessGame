using System;

namespace ChessGame
{
    public class Spot
    {
        private int column; // Rename 'x' to 'column' to represent the column index
        private int row;    // Rename 'y' to 'row' to represent the row index
        private Piece piece;

        public Spot(int row, int column, Piece piece)
        {
            this.column = column;
            this.row = row;
            this.piece = piece;
        }

        public int GetColumn() { return column; }
        public void SetColumn(int column) { this.column = column; }

        public int GetRow() { return row; }
        public void SetRow(int row) { this.row = row; }

        public Piece GetPiece() { return piece; }
        public void SetPiece(Piece piece) { this.piece = piece; }

        public override string ToString()
        {
            string pieceString = piece != null ? piece.ToString() : "Empty";
            return $"Spot [Row={row}, Column={column}, Piece={pieceString}]";
        }
    }
}
