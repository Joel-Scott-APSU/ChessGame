using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

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

        }

        public bool movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;

            Piece movingPiece = board.getSpot(fromRow, fromColumn).GetPiece();  //piece that is currently attempting to move 
            Trace.WriteLine($"CurrentPieces: {currentTurn.getPieces()}");
            if (movingPiece == null || !currentTurn.getPieces().Contains(movingPiece)) //if there is no piece moving or it is not that players turn
            {
                Trace.WriteLine($"Moving Piece: {movingPiece}");
                if (movingPiece != null)
                {
                    Trace.WriteLine($"Moving Piece Color: {movingPiece.isWhite()}");
                }
                Trace.WriteLine($"current Turn {currentTurn}");
                return false;
            }

            Spot start = board.getSpot(fromRow, fromColumn);  //starting point on the board for the selected piece
            Spot end = board.getSpot(toRow, toColumn); //ending point where selected piece is trying to move 
            Trace.WriteLine($"Start: {start} End: {end}");
            Trace.WriteLine($"Legal Move {movingPiece.legalMove(board, start, end)}");
            if (movingPiece.legalMove(board, start, end))  //if the selected pieces move is legal
            {
                Piece capturedPiece = end.GetPiece(); //piece that is attempting to be captured

                if (capturedPiece != null && capturedPiece.isWhite != movingPiece.isWhite) // check if the piece exists and if it is the opposite color of the selected piece 
                {
                    capturedPiece.setTaken(true); //set the piece that was captured to captured 

                    if (currentTurn == whitePlayer) // removes the piece based on the color of the current player 
                    {
                        blackPlayer.removePiece(capturedPiece);
                    }
                    else
                    {
                        whitePlayer.removePiece(capturedPiece);
                    }
                }

                end.SetPiece(movingPiece); // sets the end point of the moving piece to the square it moved to 
                start.SetPiece(null); //sets the start point of the piece that moved to null 

                currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;  //changes the players turn to the opposite color  

                return true;
            }
            return false;

        }
    }
}
