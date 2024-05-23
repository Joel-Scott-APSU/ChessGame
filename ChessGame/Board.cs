using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    internal class Board
    {
        Spot[][] boxes = new Spot[8][];

        public Board()
        {
            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new Spot[8];
            }

            //initializes the new board 
            this.resetBoard();
        }

        public Spot getSpot(int x, int y)
        {
            if(x < 0 || x > 7 || y < 0 || y > 7) {
                throw new ArgumentOutOfRangeException("Index out of bounds");
            }

            return boxes[x][y];
        }
        public void resetBoard()
        {
            //initialize the white king on the board 
            boxes[0][4] = new Spot(0, 4, new Piece.King(true));
            //initialize the white queen on the board 
            boxes[0][3] = new Spot(0 , 3, new Piece.Queen(true));
            //initialze the white rook on the board 
            boxes[0][0] = new Spot(0, 0, new Piece.Rook(true));
            //initialize the white knight on the board 
            boxes[0][1] = new Spot(0, 1, new Piece.Knight(true));
            //initialize the white bishop on the board 
            boxes[0][2] = new Spot(0, 2, new Piece.Bishop(true));
            //initialize all the white pawns 
            boxes[1][0] = new Spot(1, 0, new Piece.Pawn(true));
            boxes[1][1] = new Spot(1, 1, new Piece.Pawn(true));
            boxes[1][2] = new Spot(1, 2, new Piece.Pawn(true));
            boxes[1][3] = new Spot(1, 3, new Piece.Pawn(true));
            boxes[1][4] = new Spot(1, 4, new Piece.Pawn(true));
            boxes[1][5] = new Spot(1, 5, new Piece.Pawn(true));
            boxes[1][6] = new Spot(1, 6, new Piece.Pawn(true));
            boxes[1][7] = new Spot(1, 7, new Piece.Pawn(true));

            //initialize the black king on the board
            boxes[7][4] = new Spot(7, 4, new Piece.King(false));
            //initialize the black queen on the board 
            boxes[7][3] = new Spot(7, 5, new Piece.Queen(false));
            //initialize the black rook on the board 
            boxes[7][0] = new Spot(7, 0, new Piece.Rook(false));
            //initilize the black Knight on the board
            boxes[7][1] = new Spot(7,1, new Piece.Knight(false));
            //initialize the black bishop on the board
            boxes[7][2] = new Spot(7, 2, new Piece.Bishop(false));
            //initialize the black pawns on the board 
            boxes[6][0] = new Spot(6, 0, new Piece.Pawn(false));
            boxes[6][1] = new Spot(6, 1, new Piece.Pawn(false));
            boxes[6][2] = new Spot(6, 2, new Piece.Pawn(false));
            boxes[6][3] = new Spot(6, 3, new Piece.Pawn(false));
            boxes[6][4] = new Spot(6, 4, new Piece.Pawn(false));
            boxes[6][5] = new Spot(6, 5, new Piece.Pawn(false));
            boxes[6][6] = new Spot(6, 6, new Piece.Pawn(false));
            boxes[6][7] = new Spot(6, 7, new Piece.Pawn(false));

            for(int i = 2; i < 6; i++)
            {
                for(int j = 0; i < 8; i++)
                {
                    boxes[i][j] = new Spot(i, j, null);
                }
            }


        }
    }
}
