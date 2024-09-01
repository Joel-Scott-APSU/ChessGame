using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.PortableExecutable;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ChessGame
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ChessBoardSquare> ChessBoardSquares { get; set; }
        
        private ChessBoardSquare selectedSquare;
        private Game game;

        public MainWindowViewModel()
        {
            ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
            InitializeChessBoard();
            game = new Game();
            InitializeChessBoardNumbers();
        }

        private void InitializeChessBoard()
        {
            bool isWhiteSquare = true;
            SolidColorBrush darkSquareBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A3B2C"));

            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                        var square = new ChessBoardSquare(i,j)
                        {
                            
                            isWhiteSquare = isWhiteSquare, // Set the property here
                            Background = new VisualBrush
                            {
                                Visual = new Grid
                                {
                                    Children =
                                {
                                    new Image
                                    {
                                        Source = new BitmapImage(new Uri("pack://application:,,,/Textures/WoodTexture.jpg")),
                                        Stretch = Stretch.Fill
                                    },
                                    new Rectangle
                                    {
                                        Fill = (isWhiteSquare ? Brushes.White : darkSquareBrush),
                                        Opacity = 0.5,
                                    }
                                }
                                }
                            },
                            PieceImage = GetInitialPieceImage(i, j)
                        };

                        ChessBoardSquares.Add(square);

                        isWhiteSquare = !isWhiteSquare; // Toggle the color for the next square
                    }

                    // Toggle the color for the start of the next row
                    isWhiteSquare = !isWhiteSquare;            
            }
        }

        private void InitializeChessBoardNumbers()
        {

        }

        private ImageSource GetInitialPieceImage(int row, int col)
        {
            Piece piece = GetInitialPiece(row, col);
            return GetPieceImage(piece);
        }

        private Piece GetInitialPiece(int row, int col)
        {
            if (row == 1) return new Piece.Pawn(false);  // Black pawns on the second row from top
            if (row == 6) return new Piece.Pawn(true);   // White pawns on the second row from bottom

            if (row == 0 || row == 7)
            {
                bool isWhite = row == 7; // Bottom row (7) is white, top row (0) is black

                switch (col)
                {
                    case 0: case 7: return new Piece.Rook(isWhite);
                    case 1: case 6: return new Piece.Knight(isWhite);
                    case 2: case 5: return new Piece.Bishop(isWhite);
                    case 3: return new Piece.Queen(isWhite);
                    case 4: return new Piece.King(isWhite);
                }
            }

            return null;
        }

        private ImageSource GetPieceImage(Piece piece)
        {
            if (piece == null)
            {
                return null;
            }

            string uri = $"pack://application:,,,/ChessPieces/{piece.type.ToString()}_{(piece.isWhite() ? "White" : "Black")}.png";

            try
            {
                return new BitmapImage(new Uri(uri));
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString() + uri);
                return null;
            }
        }

        public void OnSquareSelected(ChessBoardSquare square)
        {
            if (selectedSquare == null)
            {
                if (square.PieceImage != null)
                {
                    square.IsHighlighted = true;
                    selectedSquare = square;
                }
            }
            else
            {
                if (!square.IsHighlighted)
                {
                    Debug.WriteLine($"Selected Square: {selectedSquare} Square: {square}");

                    // Call movePiece and retrieve if move was successful and if en passant capture occurred
                    (bool moveSuccessful, bool enPassantCapture) = game.movePiece(selectedSquare, square);

                    Debug.WriteLine($"MoveSuccessful: {moveSuccessful}, EnPassantCapture: {enPassantCapture}");

                    if (moveSuccessful)
                    {
                        if (enPassantCapture)
                        {
                            MovePieceEnPassant(selectedSquare, square);
                        }
                        else
                        {
                            MovePiece(selectedSquare, square);
                        }
                    }
                }

                selectedSquare.IsHighlighted = false;
                selectedSquare = null;
            }
        }



        private void MovePiece(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            // Perform the move logic
            toSquare.PieceImage = fromSquare.PieceImage;
            fromSquare.PieceImage = null;

            // Update UI after move
            OnPropertyChanged(nameof(ChessBoardSquares));
        }

        private void MovePieceEnPassant(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            Player player = game.currentTurn;

            // Determine the direction based on the player's color
            int direction = player.IsWhite ? -1 : 1; // Correct direction for en passant

            // Calculate the row of the adjacent square for en passant
            int enPassantRow = toSquare.row + direction;

            // Use FindSquare to locate the en passant square
            ChessBoardSquare enPassantSquare = FindSquare(enPassantRow, toSquare.column);
            Debug.WriteLine($"En Passant Square: {enPassantSquare}");

            // If the enPassantSquare is found, clear the piece image from it
            if (enPassantSquare != null)
            {
                enPassantSquare.PieceImage = null; // This should clear the image from the UI
            }

            // Move the piece image to the toSquare
            toSquare.PieceImage = fromSquare.PieceImage;
            fromSquare.PieceImage = null;

            // Notify that the ChessBoardSquares collection has changed, triggering a UI update
            OnPropertyChanged(nameof(ChessBoardSquares));
        }



        // Helper method to find a ChessBoardSquare by its row and column
        private ChessBoardSquare FindSquare(int row, int column)
        {
            // Search for the square in the ChessBoardSquares collection
            return ChessBoardSquares.FirstOrDefault(square => square.row == row && square.column == column);
        }


        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
