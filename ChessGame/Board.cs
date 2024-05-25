using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ChessGame
{
    internal class Board
    {
        private bool[,] threatMap;
        Spot[][] boxes = new Spot[8][];

        public Board()
        {
            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new Spot[8];
            }

            threatMap = new bool[8,8];
            //initializes the new board 
            this.resetBoard();


        }

        //using list of pieces to create the threat map based on the position of the opponents pieces at the beginning of each turn
        public void updateThreatMap(List<Piece> opponentPieces)
        {
            ClearThreatMap();

            foreach (Piece piece in opponentPieces)
            {
               MarkThreats(piece);
            }
        }

        //clears the threat map so it can be reinitialized each turn 
        private void ClearThreatMap()
        {
            for(int i = 0; i < 8;i++)
            {
                for(int j = 0; j < 8; j++)
                {
                    threatMap[i,j] = false;
                }
            }
        }

        private void MarkThreats(Piece piece)
        {
            if (piece.isTaken())
            {
                return;
            }

            Spot position = null;
            for(int i = 0;i < 8; i++)
            {
                for(int j =0; j < 8; j++)
                {
                    if (boxes[i][j].GetPiece() == piece)
                    {
                        position = boxes[i][j];
                        break;
                    }
                }
                if (position != null) break;
            }

            if(position == null)
            {
                throw new InvalidOperationException("Piece not found on the board");
            }

            int x = position.getX();
            int y = position.getY();


        }

        private void markPawnThreats(Piece pawn, int x, int y)
        {
            int direction = pawn.isWhite() ? 1 : -1;

            if(x + direction >= 0 && x + direction < 8)
            {
                if(y - 1 >= 0)
                {
                    threatMap[x + direction, y - 1] = true;
                }
                if(y + 1 < 8)
                {
                    threatMap[x + direction, y + 1] = true;
                }
            }            
        }

        private void markRookThreats(Piece rook, int x, int y)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionStraight(rook, x, y, direction);
            }
        }

        private void markBishopThreats(Piece Bishop, int x, int y)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionDiagonal(Bishop, x, y, direction);
            }
        }

        private void markQueenThreats(Piece Queen, int x, int y)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionStraight(Queen, x, y, direction);
            }

            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionDiagonal(Queen, x, y, direction);
            }
        }

        private void MarkKnightThreats(Piece Knight, int x, int y)
        {
            int[] dx = {2, 2, -2, -2, 1, -1, -1, 1};
            int[] dy = {-1, 1, -1, 1, 2, 2, -2, -2 };

            for (int i = 0; i < dx.Length; i++)
            {
                int newX = x + dx[i];
                int newY = y + dy[i];

                if(newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
                {
                    Spot spot = getSpot(newX, newY);
                    if(spot != null)
                    {
                        threatMap[newX, newY] = true;
                    }
                }
            }
        }
        private void MarkThreatsInDirectionStraight(Piece piece, int x, int y, Piece.Direction direction)
        {
            int dx = 0;
            int dy = 0;

            switch(direction)
            {
                case Piece.Direction.North:
                    dx = -1;
                    break;
                case Piece.Direction.South:
                    dx = 1;
                    break;
                case Piece.Direction.East:
                    dy = 1; 
                    break;
                case Piece.Direction.West:
                    dy = -1;
                    break;
                default:
                    return;

            }

            int newX = x + dx;
            int newY = y + dy;

            while(newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                threatMap[newX, newY] = true;
                Spot spot = getSpot(newX, newY);
                if (spot.GetPiece != null)
                {
                    break;
                }

                newX += dx;
                newY += dy;
            }
        }

        private void MarkThreatsInDirectionDiagonal(Piece piece, int x, int y, Piece.Direction direction)
        {
            int dx = 0;
            int dy = 0;

            switch (direction)
            {
                case Piece.Direction.Northeast:
                    dx = -1; dy = 1;
                    break;
                case Piece.Direction.Northwest:
                    dx = -1; dy = -1;
                    break;
                case Piece.Direction.Southeast:
                    dx = 1; dy = 1;
                    break;
                case Piece.Direction.Southwest:
                    dx = 1; dy = -1;
                    break;
                default:
                    return;
            }

            int newX = x + dx;
            int newY = y + dy;

            while(newX >= 0 && newX < 8 && newY >= 0 && newY < 8)
            {
                //set the spot on the threat map to true 
                threatMap[newX, newY] = true;
                Spot spot = getSpot(newX, newY);
                if(spot != null)
                {
                    break;
                }

                newX += dx;
                newY += dy;
            }
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
