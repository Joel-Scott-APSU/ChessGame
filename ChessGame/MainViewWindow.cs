using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
        public Board GameBoard { get; private set; }
        private ChessBoardSquare selectedSquare;
        private Game game;
        public MainWindowViewModel()
        {
            ChessBoardSquares = new ObservableCollection<ChessBoardSquare>();
            InitializeChessBoard();
        }

        private void InitializeChessBoard()
        {
            bool isWhite = false;
            SolidColorBrush myBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#6A3B2C"));

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    var square = new ChessBoardSquare
                    {
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
                            new Rectangle { Fill = isWhite ? Brushes.White : myBrush, Opacity = 0.5 }
                        }
                            }
                        },
                        PieceImage = null // or set your default image here
                    };

                    ChessBoardSquares.Add(square);

                    isWhite = !isWhite; // Toggle the color for the next square
                }

                // Flip the color for the next row
                isWhite = !isWhite;
            }
        }

        private ImageSource getPieceImage(Piece piece)
        {
            if(piece == null)
            {
                return null;
            }

            string uri = $"pack://application:,,,/ChessPieces/{piece.type.ToString()}_{(piece.isWhite() ? "White" : "Black")}.png";

            return new BitmapImage(new Uri(uri));
        }

        private void onSquareSelected(ChessBoardSquare square)
        {
            if(selectedSquare == null)
            {
                selectedSquare = square;
            }

            else
            {
                if (game.movePiece(game.selectedSquare, square)){
                    updateUIForMove(game.selectedSquare, square);
                }

                game.selectedSquare = null;
            }


        }

        private void updateUIForMove(ChessBoardSquare fromSquare, ChessBoardSquare toSquare)
        {
            toSquare.PieceImage = fromSquare.PieceImage;

            fromSquare.PieceImage = null;
        }

    }
}
