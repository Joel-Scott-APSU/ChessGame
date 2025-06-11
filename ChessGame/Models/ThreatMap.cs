using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChessGame.Core;

namespace ChessGame.Models
{

    public class ThreatMap
    {
        private bool[,] threatMap;
        private Player whitePlayer;
        private Player blackPlayer;
        private Game game;
        private GameRules gameRules;
        private Board board => game.board;

        public ThreatMap(Player WhitePlayer, Player BlackPlayer, Game game)
        {
            whitePlayer = WhitePlayer;
            blackPlayer = BlackPlayer;
            this.game = game;
            gameRules = GameRules.GetInstance(game, null);

            threatMap = new bool[8, 8]; // Initialize the threat map
        }
        public bool willMovePutKingInCheck(Spot start, Spot end, bool isWhite)
        {
            Piece movingPiece = start.Piece;
            if (movingPiece == null)
                return false;

            Piece capturedPiece = end.Piece;
            Spot originalPosition = movingPiece.getCurrentPosition();

            // Simulate move
            start.Piece = null;
            end.Piece = movingPiece;
            gameRules.CapturePiece(capturedPiece); //remove captured piece from active pieces

            movingPiece.setCurrentPosition(end);
            bool kingInCheck = IsKingInCheck(isWhite);

            // Undo move
            start.Piece = movingPiece;
            end.Piece = capturedPiece;
            movingPiece.setCurrentPosition(originalPosition);
            gameRules.AddActivePiece(capturedPiece); // Restore captured piece to active pieces
            return kingInCheck;
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
                        Spot spot = board.GetSpot(i, j);
                        if (spot.Piece == piece)
                        {
                            position = spot;
                            break;
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
                    Spot spot = board.GetSpot(newRow, newColumn);
                    if (spot.Piece != null)
                    {
                        threatMap[newRow, newColumn] = true;
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
                Spot spot = board.GetSpot(newRow, newColumn);
                if (spot?.Piece != null)
                {
                    if (spot.Piece.type == Piece.PieceType.King)
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
                Spot spot = board.GetSpot(newRow, newColumn);
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
            Spot kingSpot = board.FindKing(isWhite);

            Debug.WriteLine("isWhite: " + isWhite);

            UpdateThreatMap(game.gameRules.GetActivePieces(!isWhite));

            IEnumerable<Piece> pieces = game.gameRules.GetActivePieces(isWhite);
            foreach (Piece piece in pieces)
            {
                Debug.WriteLine("Checking piece: " + piece);
            }

            return threatMap[kingSpot.Row, kingSpot.Column];
        }

        public bool IsSquareUnderThreat(bool isWhite, int row, int col)
        {
            UpdateThreatMap(game.gameRules.GetActivePieces(!isWhite));

            return threatMap[row, col];
        }
    }
}
