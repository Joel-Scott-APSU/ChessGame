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
        public Moves moves { get; private set; }

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
            moves = new Moves(whitePlayer, blackPlayer);
        }

        public bool movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Piece piece;

            int fromRow = fromSquare.row;
            int fromColumn = fromSquare.column;
            int toRow = toSquare.row;
            int toColumn = toSquare.column;
            Piece capturedPiece = null;


            Spot start = board.getSpot(fromRow, fromColumn);  // Starting point on the board for the selected piece
            Spot end = board.getSpot(toRow, toColumn); // Ending point where selected piece is trying to move 

            Piece movingPiece = start.GetPiece();  // Piece that is currently attempting to move 
            
            if(movingPiece == null || movingPiece.isWhite() != currentTurn.IsWhite)
            {
                return false;
            }

            if(!board.isMoveValid(start, end, movingPiece.isWhite()))
            {
                Debug.WriteLine("Move will put king into check");
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

                board.updateThreatMap(currentTurn.getPieces());

                if (board.isKingInCheck(currentTurn.IsWhite)){
                    Debug.WriteLine($"King is in Check, running checkForLegalMoves function");
                    if (!moves.checkForLegalMoves(currentTurn.IsWhite, board))
                    {
                        Debug.WriteLine("CHECKMATE");
                    }
                }
                currentTurn = currentTurn == whitePlayer ? blackPlayer : whitePlayer;  // Change the player's turn to the opposite color  


                Debug.WriteLine($"\nCurrent Player {currentTurn}\n");

                return true;
            }
            return false;
        }

        private bool DrawScenarios()
        {
            return false;
        }
    }
}
