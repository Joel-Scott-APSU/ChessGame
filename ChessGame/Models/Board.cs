using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ChessGame.Models
{
    public class Board
    {
        private Spot[][] boxes = new Spot[8][];
        private Player whitePlayer;
        private Player blackPlayer;
        private Game game;

        public Board(Player whitePlayer, Player blackPlayer, Game game)
        {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;
            this.game = game;

            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new Spot[8];
                for (int j = 0; j < 8; j++)
                {
                    boxes[i][j] = new Spot(i, j, null);
                }
            }

            ResetBoard();
        }

        // === Board Setup ===

        public void ResetBoard()
        {
            // White pieces
            CreatePieces(new Piece.King(true), 7, 4, whitePlayer);
            //CreatePieces(new Piece.Queen(true), 7, 3, whitePlayer);
            //CreatePieces(new Piece.Rook(true), 7, 0, whitePlayer);
            //CreatePieces(new Piece.Rook(true), 7, 7, whitePlayer);
            //CreatePieces(new Piece.Knight(true), 7, 1, whitePlayer);
            //CreatePieces(new Piece.Knight(true), 7, 6, whitePlayer);
            //CreatePieces(new Piece.Bishop(true), 7, 2, whitePlayer);
            CreatePieces(new Piece.Bishop(true), 7, 5, whitePlayer);
            //for (int i = 0; i < 8; i++)
               // CreatePieces(new Piece.Pawn(true), 6, i, whitePlayer);*/

            // Black pieces
            CreatePieces(new Piece.King(false), 0, 4, blackPlayer);
            /*CreatePieces(new Piece.Queen(false), 0, 3, blackPlayer);
            CreatePieces(new Piece.Rook(false), 0, 0, blackPlayer);
            CreatePieces(new Piece.Rook(false), 0, 7, blackPlayer);
            CreatePieces(new Piece.Knight(false), 0, 1, blackPlayer);
            CreatePieces(new Piece.Knight(false), 0, 6, blackPlayer);
            CreatePieces(new Piece.Bishop(false), 0, 2, blackPlayer);
            CreatePieces(new Piece.Bishop(false), 0, 5, blackPlayer);
            for (int i = 0; i < 8; i++)
                CreatePieces(new Piece.Pawn(false), 1, i, blackPlayer);*/

            // Clear middle rows
            for (int i = 2; i < 6; i++)
                for (int j = 0; j < 8; j++)
                    boxes[i][j] = new Spot(i, j, null);
        }

        public void clearBoard()
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    boxes[i][j] = new Spot(i, j, null);
        }

        public void CreatePieces(Piece piece, int row, int col, Player player)
        {
            boxes[row][col] = new Spot(row, col, piece);
            player.addPiece(piece);
            piece.setCurrentPosition(boxes[row][col]);
        }

        // === Piece and Spot Utilities ===

        public void emptyPieces(List<Piece> pieces)
        {
            pieces.Clear();
        }

        public Spot GetSpot(int row, int col)
        {
            if (row < 0 || row > 7 || col < 0 || col > 7)
                throw new ArgumentOutOfRangeException("Index out of bounds");

            return boxes[row][col];
        }

        public Spot FindKing(bool isWhite)
        {
            IEnumerable<Piece> pieces = game.gameRules.GetActivePieces(isWhite);
            foreach (Piece piece in pieces)
            {
                if (piece is Piece.King)
                    return piece.getCurrentPosition();
            }
            throw new InvalidOperationException("King not found on the board");
        }
    }
}
