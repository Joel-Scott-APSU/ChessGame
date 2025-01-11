using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Packaging;
using System.Data.Common;

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

        private GameRules gameRules;

        public Game()
        {
            initializeGame();
            gameRules = new GameRules(this);
        }

        private void initializeGame()
        {
            whitePlayer = new Player(true);
            blackPlayer = new Player(false);
            board = new Board(whitePlayer, blackPlayer);
            moves = new Moves(whitePlayer, blackPlayer);
            currentTurn = whitePlayer;
        }

        public (bool moveSuccessful, bool enPassantCaptureOccurred, bool CastledKingSide, bool CastledQueenSide) movePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            return gameRules.HandleMove(fromSquare, toSquare);
        }

        public void SetCurrentTurn(Player newTurn)
        {
            currentTurn = newTurn;
        }

    }
}




