﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ChessGame
{
    public class Board
    {
        private bool[,] threatMap;
        private Spot[][] boxes = new Spot[8][];
        private Player whitePlayer;
        private Player blackPlayer;
        private GameRules gameRules;

        public Board(Player whitePlayer, Player blackPlayer, GameRules gameRules)
        {
            this.whitePlayer = whitePlayer;
            this.blackPlayer = blackPlayer;

            for (int i = 0; i < 8; i++)
            {
                boxes[i] = new Spot[8];
                for (int j = 0; j < 8; j++)
                {
                    boxes[i][j] = new Spot(i, j, null); // Initialize all spots as null
                }
            }

            threatMap = new bool[8, 8];
            // Initializes the new board
            ResetBoard();
            this.gameRules = gameRules;
        }

        public bool willMovePutKingInCheck(Spot start, Spot end, bool isWhite)
        {
            //saves the original pieces to move them back after the simulation 
            Piece originalStartPiece = start.Piece;
            Piece originalEndPiece = end.Piece;

            //moves the pieces to simulate the movement and check if the king is in check 
            end.Piece = originalStartPiece;
            start.Piece = null;

            //check if the move puts the king in check
            bool kingInCheck = IsKingInCheck(isWhite);
            Debug.WriteLine($"Result: {(kingInCheck ? "King is in check" : "King is safe")}");


            //reverts the pieces back to their original position
            start.Piece = originalStartPiece;
            end.Piece = originalEndPiece;

            return !kingInCheck;
        }

        public void UpdateThreatMap(IEnumerable<Piece> opponentPieces)
        {
            ClearThreatMap();
            foreach (var piece in opponentPieces)
            {
                MarkThreats(piece);
            }
        }


        private void ClearThreatMap()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    threatMap[row, col] = false;
                }
            }
        }

        private void MarkThreats(Piece piece)
        {
            if (piece.isTaken())
            {
                return;
            }

            try
            {
                Spot position = null;

                for (int i = 0; i < 8; i++)
                {
                    for (int j = 0; j < 8; j++)
                    {
                        if (boxes[i][j] != null)
                        {
                            Piece p = boxes[i][j].Piece;
                            if (p == piece)
                            {
                                position = boxes[i][j];
                                break;
                            }
                        }
                    }
                    if (position != null) break;
                }

                if (position == null)
                {
                    throw new InvalidOperationException($"Piece not found on the board: {piece}");
                }


                int row = position.Row; 
                int col = position.Column; 

                switch (piece.type)
                {
                    case Piece.PieceType.Pawn:
                        markPawnThreats(piece, row, col);
                        break;
                    case Piece.PieceType.Rook:
                        markRookThreats(piece, row, col);
                        break;
                    case Piece.PieceType.Bishop:
                        markBishopThreats(piece, row, col);
                        break;
                    case Piece.PieceType.Queen:
                        markQueenThreats(piece, row, col);
                        break;
                    case Piece.PieceType.Knight:
                        MarkKnightThreats(row, col);
                        break;
                    case Piece.PieceType.King:
                        MarkKingThreats(piece, row, col);
                        break;
                }
            }
            catch (InvalidOperationException e)
            {
                Debug.WriteLine($"{e}");
            }
        }

        private void MarkKingThreats(Piece king, int row, int col)
        {
            int[] rows = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] columns = { -1, 0, 1, -1, 1, -1, 0, 1 };

            for (int i = 0; i < rows.Length; i++)
            {
                int newRow = row + rows[i];
                int newColumn = col + columns[i];

                if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8)
                {
                    threatMap[newRow, newColumn] = true;
                }
            }
        }

        private void markPawnThreats(Piece pawn, int row, int col)
        {
            int direction = pawn.isWhite() ? -1 : 1; // Pawns move in opposite directions based on color

            if (row + direction >= 0 && row + direction < 8)
            {
                if (col - 1 >= 0)
                {
                    threatMap[row + direction, col - 1] = true;
                }
                if (col + 1 < 8)
                {
                    threatMap[row + direction, col + 1] = true;
                }
            }
        }

        private void markRookThreats(Piece rook, int row, int col)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionStraight(rook, row, col, direction);
            }
        }

        private void markBishopThreats(Piece Bishop, int row, int col)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionDiagonal(Bishop, row, col, direction);
            }
        }

        private void markQueenThreats(Piece Queen, int row, int col)
        {
            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionStraight(Queen, row, col, direction);
            }

            foreach (Piece.Direction direction in Enum.GetValues(typeof(Piece.Direction)))
            {
                MarkThreatsInDirectionDiagonal(Queen, row, col, direction);
            }
        }

        private void MarkKnightThreats(int row, int col)
        {
            int[] rows = { 2, 2, 1, -1, -2, -2, 1, -1 };
            int[] columns = { -1, 1, -2, -2, -1, 1, 2, 2 };

            for (int i = 0; i < rows.Length; i++)
            {
                int newRow = row + rows[i];
                int newColumn = col + columns[i];

                if (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8)
                {
                    Spot spot = GetSpot(newRow, newColumn);
                    if (spot.Piece != null)
                    {
                        if (spot.Piece.type == Piece.PieceType.King)
                        {
                            threatMap[newRow, newColumn] = true;
                        }
                    }
                }
            }
        }

        private void MarkThreatsInDirectionStraight(Piece piece, int row, int col, Piece.Direction direction)
        {
            int Rows = 0;
            int Columns = 0;

            switch (direction)
            {
                case Piece.Direction.North:
                    Rows = -1;
                    break;
                case Piece.Direction.South:
                    Rows = 1;
                    break;
                case Piece.Direction.East:
                    Columns = 1;
                    break;
                case Piece.Direction.West:
                    Columns = -1;
                    break;
                default:
                    return;
            }

            int newRow = row + Rows;
            int newColumn = col + Columns;

            while (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8)
            {
                threatMap[newRow, newColumn] = true;
                Spot spot = GetSpot(newRow, newColumn);
                if (spot?.Piece != null)
                {
                    if(spot.Piece.type == Piece.PieceType.King)
                    {
                        threatMap[newRow, newColumn] = true;
                    }
                    break;
                }

                newRow += Rows;
                newColumn += Columns;
            }
        }


        private void MarkThreatsInDirectionDiagonal(Piece piece, int row, int col, Piece.Direction direction)
        {
            int Rows = 0;
            int Columns = 0;

            switch (direction)
            {
                case Piece.Direction.Northeast:
                    Rows = -1; Columns = 1;
                    break;
                case Piece.Direction.Northwest:
                    Rows = -1; Columns = -1;
                    break;
                case Piece.Direction.Southeast:
                    Rows = 1; Columns = 1;
                    break;
                case Piece.Direction.Southwest:
                    Rows = 1; Columns = -1;
                    break;
                default:
                    return;
            }

            int newRow = row + Rows;
            int newColumn = col + Columns;

            while (newRow >= 0 && newRow < 8 && newColumn >= 0 && newColumn < 8)
            {
                // Set the spot on the threat map to true 
                threatMap[newRow, newColumn] = true;
                Spot spot = GetSpot(newRow, newColumn);
                if (spot?.Piece != null)
                {
                    break;
                }

                newRow += Rows;
                newColumn += Columns;
            }
        }

        public bool IsKingInCheck(bool isWhite)
        {
            Spot kingSpot = FindKing(isWhite);

            UpdateThreatMap(gameRules.GetActivePieces(!isWhite));

            return threatMap[kingSpot.Row, kingSpot.Column];
        }

        public bool IsSquareUnderThreat(bool isWhite, int row, int col)
        {
            UpdateThreatMap(gameRules.GetActivePieces(isWhite));

            return threatMap[row, col];
        }

        public Spot FindKing(bool isWhite)
        {
            IEnumerable<Piece> pieces = gameRules.GetActivePieces(isWhite);

            foreach(Piece piece in pieces)
            {
                if(piece is Piece.King)
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
            if(piece.isWhite())
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
    }
}
