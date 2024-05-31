using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    public class Spot
    {
        private int x;
        private int y;
        private Piece piece;


        internal Spot(int x, int y, Piece piece)
        {
            this.setX(x);
            this.y = y;
            this.piece = piece;
        }

        public int getX() { return x; }
        public void setX(int x) { this.x = x; }
        public int getY() { return y; }
        public void setY(int y) {  this.y = y; }

        public Piece GetPiece() { return piece; }

        public void setPiece(Piece piece) { this.piece = piece; }
        
    }
}
