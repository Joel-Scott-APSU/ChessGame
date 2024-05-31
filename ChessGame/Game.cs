using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessGame
{
    public class Game
    {
        public Board board { get; private set; }
        public Player whitePlayer { get; private set; }
        public Player blackPlayer { get; private set; }
        public Player currentTurn { get; private set; }
        public ChessBoardSquare selectedSquare { get; set; }

        public Game()
        {
            initializeGame();
        }

        private void initializeGame()
        {
            board = new Board();
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
            currentTurn = whitePlayer;

            board.resetBoard();

        }

        public bool movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            int fromX = fromSquare.X;
            int fromY = fromSquare.Y;
            int toX = toSquare.X;
            int toY = toSquare.Y;

            Piece movingPiece = board.getSpot(fromX, fromY).GetPiece();

            if (movingPiece == null || !currentTurn.getPieces().Contains(movingPiece))
            {
                return false;
            }

            Spot start = board.getSpot(fromX, fromY);
            Spot end = board.getSpot(toX, toY);

            if (movingPiece.legalMove(board, start, end))
            {
                Piece capturedPiece = end.GetPiece();

                if (capturedPiece != null && capturedPiece.isWhite != movingPiece.isWhite)
                {
                    capturedPiece.setTaken(true);

                    if (currentTurn == whitePlayer)
                    {
                        blackPlayer.removePiece(capturedPiece);
                    }
                    else
                    {
                        whitePlayer.removePiece(capturedPiece);
                    }
                }

                end.setPiece(movingPiece);
                start.setPiece(null);

                currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;

                return true;
            }

            return false;

        }
    }
}
