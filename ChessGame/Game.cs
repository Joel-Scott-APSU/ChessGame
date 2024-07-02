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
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
            board = new Board(whitePlayer, blackPlayer);
            currentTurn = whitePlayer;

        }

        public bool movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;
            Piece capturedPiece = null;

            Debug.WriteLine($"from square color: {fromSquare.isWhiteSquare}");
            Debug.WriteLine($"Current Player {currentTurn}\n");

            Spot start = board.getSpot(fromRow, fromColumn);  // Starting point on the board for the selected piece
            Spot end = board.getSpot(toRow, toColumn); // Ending point where selected piece is trying to move 
            Trace.WriteLine($"Start: {start} End: {end}");

            Piece movingPiece = start.GetPiece();  // Piece that is currently attempting to move 
            Trace.WriteLine($"Legal Move {movingPiece.legalMove(board, start, end)}");

            if(movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return false;
            }

            if (movingPiece.legalMove(board, start, end))  // If the selected piece's move is legal
            {
                if (end.GetPiece() != null)
                {
                    capturedPiece = end.GetPiece(); // Piece that is attempting to be captured
                }

                if (capturedPiece != null && capturedPiece.isWhite() != movingPiece.isWhite()) // Check if the piece exists and if it is the opposite color of the selected piece 
                {
                    capturedPiece.setTaken(true); // Set the piece that was captured to captured 

                    if (currentTurn == whitePlayer) // Removes the piece based on the color of the current player 
                    {
                        blackPlayer.removePiece(capturedPiece);
                    }
                    else
                    {
                        whitePlayer.removePiece(capturedPiece);
                    }
                }

                end.SetPiece(movingPiece);
                start.SetPiece(null);
                currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;  // Change the player's turn to the opposite color  

                return true;
            }
            return false;
        }

    }
}
