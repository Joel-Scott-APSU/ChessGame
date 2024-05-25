using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace ChessGame
{
    internal class Board
    {
        private bool[,] threatMap;
        private Spot[][] boxes = new Spot[8][];
        private Player whitePlayer;
        private Player blackPlayer;

        public Board()
        {
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);

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

        public bool inKingInCheck(bool isWhite)
        {
            Spot Kingspot = findKing(isWhite);

            List<Piece> opponentPieces = isWhite ? blackPlayer.getPieces() : whitePlayer.getPieces();
            updateThreatMap(opponentPieces);

            return threatMap[Kingspot.getX(), Kingspot.getY()];
        }

        private Spot findKing(bool isWhite)
        {
            Player player = isWhite ? whitePlayer : blackPlayer;

            foreach (Spot[] row in boxes)
            {
                foreach (Spot spot in row)
                {
                    if(spot !=  null && spot.GetPiece() != null && spot.GetPiece().GetType() == typeof(Piece.King) && spot.GetPiece().isWhite() == isWhite)
                    {
                        return spot;
                    }
                }
            }

            return null;
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
            createPieces(new Piece.King(true), 0, 4, whitePlayer);
            //initialize the white queen on the board 
            createPieces(new Piece.Queen(true), 0, 3, whitePlayer);
            //initialize the white rooks on the board 
            createPieces(new Piece.Rook(true), 0, 0, whitePlayer);
            createPieces(new Piece.Rook(true), 0, 7, whitePlayer);
            //initialize the white knights on the board 
            createPieces(new Piece.Knight(true), 0, 1, whitePlayer);
            createPieces(new Piece.Knight(true), 0, 6, whitePlayer);
            //initialize the white bishops on the board 
            createPieces(new Piece.Bishop(true), 0, 2, whitePlayer);
            createPieces(new Piece.Bishop(true), 0, 5, whitePlayer);
            //initialize all the white pawns 
            for(int i = 0; i < 8; i++)
            {
                createPieces(new Piece.Pawn(true), 1, i, whitePlayer);
            }

            //initialize the black king on the board
            createPieces(new Piece.King(false), 7, 4, blackPlayer);
            //initialize the black queen on the board 
            createPieces(new Piece.Queen(false), 7, 3, blackPlayer);
            //initialize the black rook on the board 
            createPieces(new Piece.Rook(false), 7, 0, blackPlayer);
            createPieces(new Piece.Rook(false), 7, 7, blackPlayer);
            //initialize the black Knight on the board
            createPieces(new Piece.Knight(false), 7, 1, blackPlayer);
            createPieces(new Piece.Knight(false), 7, 6, blackPlayer);
            //initialize the black bishop on the board
            createPieces(new Piece.Bishop(false), 7, 2, blackPlayer);
            createPieces(new Piece.Bishop(false), 7, 5, blackPlayer);
            //initialize the black pawns on the board 
            for(int i = 0; i < 8; i++)
            {
                createPieces(new Piece.Pawn(false), 6, i, blackPlayer);
            }

            for(int i = 2; i < 6; i++)
            {
                for(int j = 0; i < 8; i++)
                {
                    boxes[i][j] = new Spot(i, j, null);
                }
            }


        }

        public void createPieces(Piece piece, int x, int y, Player player)
        {
            boxes[x][y] = new Spot(x, y, piece);
            player.addPiece(piece);
        }
    }
}
