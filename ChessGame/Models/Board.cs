using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

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
                    boxes[i][j] = new Spot(i, j, null); // Initialize all spots as null
                }
            }
            // Initializes the new board
            ResetBoard();
        }

        public void ResetBoard()
        {
            // Initialize the white king on the board 
            CreatePieces(new Piece.King(true), 7, 4, whitePlayer);
            // Initialize the white queen on the board 
            CreatePieces(new Piece.Queen(true), 7, 3, whitePlayer);
            // Initialize the white rooks on the board 
            CreatePieces(new Piece.Rook(true), 7, 0, whitePlayer);
            CreatePieces(new Piece.Rook(true), 7, 7, whitePlayer);
            // Initialize the white knights on the board 
            CreatePieces(new Piece.Knight(true), 7, 1, whitePlayer);
            CreatePieces(new Piece.Knight(true), 7, 6, whitePlayer);
            // Initialize the white bishops on the board 
            CreatePieces(new Piece.Bishop(true), 7, 2, whitePlayer);
            CreatePieces(new Piece.Bishop(true), 7, 5, whitePlayer);
            // Initialize all the white pawns 
            for (int i = 0; i < 8; i++)
            {
                CreatePieces(new Piece.Pawn(true), 6, i, whitePlayer);
            }

            // Initialize the black king on the board
            CreatePieces(new Piece.King(false), 0, 4, blackPlayer);
            // Initialize the black queen on the board 
            CreatePieces(new Piece.Queen(false), 0, 3, blackPlayer);
            // Initialize the black rook on the board 
            CreatePieces(new Piece.Rook(false), 0, 0, blackPlayer);
            CreatePieces(new Piece.Rook(false), 0, 7, blackPlayer);
            // Initialize the black Knight on the board
            CreatePieces(new Piece.Knight(false), 0, 1, blackPlayer);
            CreatePieces(new Piece.Knight(false), 0, 6, blackPlayer);
            // Initialize the black bishop on the board
            CreatePieces(new Piece.Bishop(false), 0, 2, blackPlayer);
            CreatePieces(new Piece.Bishop(false), 0, 5, blackPlayer);
            // Initialize the black pawns on the board 
            for (int i = 0; i < 8; i++)
            {
                CreatePieces(new Piece.Pawn(false), 1, i, blackPlayer);
            }

            for (int i = 2; i < 6; i++)
            {
                for (int j = 0; j < 8; j++)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    boxes[i][j] = new Spot(i, j, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
            }
        }

        public void CreatePieces(Piece piece, int row, int col, Player player)
        {
            boxes[row][col] = new Spot(row, col, piece);
            player.addPiece(piece);
            Spot currentPosition = boxes[row][col];
            piece.setCurrentPosition(currentPosition);
        }



        public void clearBoard()
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    boxes[i][j] = new Spot(i, j, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }
            }


        }

        public void emptyPieces(List<Piece> pieces)
        {
            pieces.Clear();
        }

        public void capturePiece(Piece piece)
        {
            if (piece.isWhite())
            {
                piece.setTaken(true);
                whitePlayer.removePiece(piece);
            }
            else
            {
                piece.setTaken(true);
                blackPlayer.removePiece(piece);
            }
        }

        public Spot FindKing(bool isWhite)
        {
            IEnumerable<Piece> pieces = game.gameRules.GetActivePieces(isWhite);

            foreach (Piece piece in pieces)
            {
                if (piece is Piece.King)
                {
                    return piece.getCurrentPosition();
                }
            }

            throw new InvalidOperationException("King not found on the board");
        }

        public Spot GetSpot(int row, int col)
        {
            if (row < 0 || row > 7 || col < 0 || col > 7)
            {
                throw new ArgumentOutOfRangeException("Index out of bounds");
            }
            return boxes[row][col];
        }
    }
}
